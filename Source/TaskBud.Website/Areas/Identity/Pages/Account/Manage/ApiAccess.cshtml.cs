using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskBud.Business.Extensions;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Website.Areas.Identity.Pages.Account.Manage
{
    public partial class ApiAccessModel : PageModel
    {
        private IApiTokenManager TokenManager { get; }

        public ApiAccessModel(IApiTokenManager tokenManager)
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
