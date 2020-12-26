using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using TaskBud.Business.Services;

namespace TaskBud.Website.Services
{
    public class ApiTokenAuthMiddleWare : IMiddleware
    {
        private ApiTokenManager TokenManager { get; }
        private UserManager<IdentityUser> UserManager { get; }

        public ApiTokenAuthMiddleWare(ApiTokenManager tokenManager, UserManager<IdentityUser> userManager)
        {
            TokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (await TryAuthorizeAsync(context))
            {
                await next(context);
                return;
            }

            // Unauthorized and trying to access API
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Missing valid Authorization header");
        }

        private async Task<bool> TryAuthorizeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.ToString();
                // Only apply our authorization to api end points, excluding the swagger endpoint
                if (!path.StartsWith("/api/") || path == "/api/index.html")
                    return true;

                var token = context.Request.Query["Bearer"].First();

                var userId = await TokenManager.ValidateAsync(token);
                var user = await UserManager.FindByIdAsync(userId);

                context.Items["User"] = user;

                var userRole = (await UserManager.GetRolesAsync(user)).Single();
                var userName = user.Id;

                // Identity Principal
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim(ClaimTypes.NameIdentifier, userName),
                };
                var identity = new ClaimsIdentity(claims, "basic");
                context.User = new ClaimsPrincipal(identity);

                return true;
            }
            catch
            {
                    return false;
            }
        }
    }
}
