using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBud.Business.Data;

namespace TaskBud.Business.Services
{
    public class DBMigrator
    {
        private TaskBudConfig Config { get; }
        private TaskBudDbContext DbContext { get; }
        private UserManager<IdentityUser> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }

        public DBMigrator(TaskBudConfig config, TaskBudDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await DbContext.Database.MigrateAsync(cancellationToken);

            _ = await TryAddAdminUserRole(cancellationToken);
            _ = await TryAddAdminUser(cancellationToken);

            await DbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<IdentityRole> TryAddAdminUserRole(CancellationToken cancellationToken)
        {
            var admin = await RoleManager.FindByNameAsync("Administrator");

            if (admin == null)
            {
                admin = new IdentityRole("Administrator");
                _ = await RoleManager.CreateAsync(admin);
            }

            return admin;
        }

        private async Task<IdentityUser> TryAddAdminUser(CancellationToken cancellationToken)
        {
            var admin = await UserManager.FindByNameAsync("Admin");
            if (admin == null)
            {
                admin = new IdentityUser("Admin")
                {
                    Email = "admin@admin.admin",
                    EmailConfirmed = true,
                };
                _ = await UserManager.CreateAsync(admin, "admin");
                _ = await UserManager.AddToRoleAsync(admin, "Administrator");
            }

            return admin;
        }
    }
}
