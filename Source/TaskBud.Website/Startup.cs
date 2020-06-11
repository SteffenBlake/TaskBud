using System;
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
using TaskBud.Website.Hubs;

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
            var config = Configuration.Get<TaskBudConfig>();
            services.AddSingleton(config);

            services.AddLogging();

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

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddSingleton(new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            // Business Services
            services.AddTransient<DBMigrator>();
            services.AddTransient<InvitationManager>();
            services.AddTransient<TaskGroupManager>();
            services.AddTransient<TaskManager>();
            services.AddTransient<PriorityHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Auto migrate EFCore Database
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var migrator = serviceScope.ServiceProvider.GetRequiredService<DBMigrator>();
                using (var cancellation = new CancellationTokenSource())
                {
                    migrator.ExecuteAsync(cancellation.Token).Wait();
                }
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
        }
    }
}
