using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskBud.Business.Data;
using TaskBud.Business;
using TaskBud.Business.Services;
using System.Threading;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TaskBud.Business.Hubs;
using TaskBud.Website.Services;
using TaskBud.Website.Swagger;

namespace TaskBud.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Config
            var config = Configuration.Get<TaskBudConfig>();

            if (config.ConnectionType == ConnectionType.INVALID)
            {
                Console.Error.WriteLine("Please specify a valid ConnectionType in your AppSettings");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(config.ConnectionString.Trim()))
            {
                Console.Error.WriteLine("Please specify a valid ConnectionString in your AppSettings");
                Environment.Exit(1);
            }

            services.AddSingleton(config);
            services.AddLogging();

            // Business Services
            services
                .AddDbContext<TaskBudDbContext>(options =>
                {
                    _ = config.ConnectionType switch
                    {
                        ConnectionType.MSSQL => options.UseSqlServer(config.ConnectionString),
                        ConnectionType.POSTGRES => options.UseNpgsql(config.ConnectionString),
                        _ => throw new ArgumentOutOfRangeException(nameof(config.ConnectionType))
                    };
                });

            services
                .AddDefaultIdentity<IdentityUser>(options => 
                { 
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.User.RequireUniqueEmail = false;
                    options.Password = config.Identity.Password;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TaskBudDbContext>();

            services.AddTransient<ApiTokenManager>();
            services.AddTransient<DBMigrator>();
            services.AddTransient<HistoryManager>();
            services.AddTransient<InvitationManager>();
            services.AddTransient<TaskGroupManager>();
            services.AddTransient<TaskManager>();

            // Front-end Services
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddSwaggerGen(gen =>
            {
                gen.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "TaskBud API",
                    Description = "API integration for TaskBud",
                    TermsOfService = new Uri("https://github.com/SteffenBlake/TaskBud/blob/master/LICENSE"),
                    Contact = new OpenApiContact
                    {
                        Name = "Steffen Blake",
                        Email = "steffen.cole.blake@gmail.com",
                        Url = new Uri("https://github.com/SteffenBlake/TaskBud"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT License",
                        Url = new Uri("https://github.com/SteffenBlake/TaskBud/blob/master/LICENSE"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                gen.IncludeXmlComments(xmlPath);

                gen.DocumentFilter<ApiSwaggerFilter>();
                gen.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. [See here](/Identity/Account/Manage/ApiAccess)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                gen.OperationFilter<ApiAuthenticationOperationFilter>();
            });

            services.AddTransient<PriorityHelper>();
            services.AddSingleton(new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
            services.AddTransient<ApiTokenAuthMiddleWare>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            // Auto migrate EFCore Database
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var config = serviceScope.ServiceProvider.GetRequiredService<TaskBudConfig>();

                loggerFactory.AddFile(config.Logging.Path, config.Logging.MinimumLevel);

                var migrator = serviceScope.ServiceProvider.GetRequiredService<DBMigrator>();
                migrator.ExecuteAsync().Wait();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();

            app.UseMiddleware<ApiTokenAuthMiddleWare>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                // SignalR hubs
                endpoints.MapHub<TaskHub>(TaskHub.EndPoint);
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskBud API V1");
                c.RoutePrefix = "api";
            });

        }
    }
}
