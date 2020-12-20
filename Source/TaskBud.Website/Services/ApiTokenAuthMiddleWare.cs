using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

                var tokenHeader = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ");
                if (tokenHeader == null)
                    return false;

                if (tokenHeader.Length != 2)
                    return false;

                if (tokenHeader[0].ToLower() != "bearer")
                    return false;

                var token = tokenHeader[1];

                var userId = await TokenManager.ValidateAsync(token);

                context.Items["User"] = await UserManager.FindByIdAsync(userId);

                return true;
            }
            catch
            {
                    return false;
            }
        }
    }
}
