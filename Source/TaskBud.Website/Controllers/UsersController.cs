using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TaskBud.Website.Controllers
{
    [Route("users")]
    [Authorize(Roles ="Administrator")]
    public class UsersController : Controller
    {
        private UserManager<IdentityUser> UserManager { get; }

        public UsersController(UserManager<IdentityUser> userManager)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            return View(user);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> Edit(string userId, IdentityUser user)
        {
            user.Id = userId;
            _ = await UserManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var oldRoles = await UserManager.GetRolesAsync(user);
            await UserManager.RemoveFromRolesAsync(user, oldRoles);
            await UserManager.AddToRoleAsync(user, roleName);

            return RedirectToAction("Index");
        }
    }
}