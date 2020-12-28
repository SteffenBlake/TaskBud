using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Business.Services.Implementations
{
    public class DBMigrator : IDBMigrator
    {
        private TaskBudDbContext DbContext { get; }
        private UserManager<IdentityUser> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }

        public DBMigrator(TaskBudDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task ExecuteAsync()
        {
            await DbContext.Database.MigrateAsync();

            var adminRole = await TryCreateRoleAsync("Administrator");
            _ = await TryCreateRoleAsync("User");
            _ = await TryCreateUserAsync("Admin", adminRole.Name, "admin");

            await TryAssignUsersAsync();

            await DbContext.SaveChangesAsync();
        }

        private async Task<IdentityRole> TryCreateRoleAsync(string roleName)
        {
            var role = await RoleManager.FindByNameAsync(roleName);

            if (role != null)
                return role;

            role = new IdentityRole(roleName);
            _ = await RoleManager.CreateAsync(role);

            return role;
        }

        private async Task<IdentityUser> TryCreateUserAsync(string userName, string roleName, string password)
        {
            var admin = await UserManager.FindByNameAsync(userName);
            if (admin != null)
                return admin;

            admin = new IdentityUser(userName)
            {
                Email = $"{userName}@{userName}.{userName}",
                EmailConfirmed = true,
            };
            _ = await UserManager.CreateAsync(admin, password);
            _ = await UserManager.AddToRoleAsync(admin, roleName);

            return admin;
        }

        // New role "User" may have pre-existing users not assigned to any role that need assigning to it
        private async Task TryAssignUsersAsync()
        {
            foreach (var user in UserManager.Users.ToList())
            {
                var roles = await UserManager.GetRolesAsync(user);
                if (roles.Any())
                    // User had a role
                    continue;

                _ = await UserManager.AddToRoleAsync(user, "User");
            }
        }
    }
}
