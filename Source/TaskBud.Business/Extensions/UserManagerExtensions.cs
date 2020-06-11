using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TaskBud.Business.Extensions
{
    public static class UserManagerExtensions
    {
        public static Task<TUser> FindUserAsync<TUser>(this UserManager<TUser> self, ClaimsPrincipal user) where TUser : class
        {
            return self.FindByIdAsync(user.GetLoggedInUserId<string>());
        }

    }
}
