using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskBud.Business.Extensions;
using TaskBud.Business.Services;

namespace TaskBud.Website.Areas.Identity.Pages.Account.Manage
{
    public partial class ApiAccessModel : PageModel
    {
        private ApiTokenManager TokenManager { get; }

        public ApiAccessModel(ApiTokenManager tokenManager)
        {
            TokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        }

        public string Token { get; set; }
        public string StatusMessage { get; set; }
        public bool HasToken { get; set; }

        private void Load()
        {
            HasToken = !string.IsNullOrEmpty(Token);
            Token ??= "---";
        }

        public IActionResult OnGet()
        {
            Token = TokenManager.ForUser(User.GetLoggedInUserId<string>());
            Load();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Token = await TokenManager.GenerateAsync(User.GetLoggedInUserId<string>());
            StatusMessage = "New Token Generated!";
            Load();
            return Page();
        }
    }
}
