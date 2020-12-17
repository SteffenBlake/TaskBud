using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using TaskBud.Business.Services;

namespace TaskBud.Website.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private ILogger<RegisterModel> Log { get; }
        private SignInManager<IdentityUser> SignInManager { get; }
        private UserManager<IdentityUser> UserManager { get; }
        private InvitationManager InvitationManager { get; }
        private IEmailSender EmailSender { get; }

        public RegisterModel(
            ILogger<RegisterModel> log,
            UserManager<IdentityUser> userManager,
            InvitationManager invitationManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            Log = log;
            UserManager = userManager;
            InvitationManager = invitationManager;
            SignInManager = signInManager;
            EmailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Invitation Code")]
            public string InvitationCode { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string code = null, string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Input = new InputModel
            {
                InvitationCode = code
            };
            ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!(await InvitationManager.ValidateAsync(Input.InvitationCode)))
            {
                ModelState.AddModelError(nameof(Input.InvitationCode), "Unrecognized Invitation code, see your Administrator for details.");
            } 
            else if (!(await InvitationManager.IsExpiredAsync(Input.InvitationCode)))
            {
                ModelState.AddModelError(nameof(Input.InvitationCode), "Expired Invitation code, see your Administrator for details.");
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.UserName };
                var userResult = await UserManager.CreateAsync(user, Input.Password);
                if (userResult.Succeeded)
                {
                    var roleResult = await UserManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        var codeResult = await InvitationManager.TryConsumeAsync(Input.InvitationCode, Input.UserName);

                        if (codeResult.Succeeded)
                        {
                            Log.LogInformation("User created a new account with password.");

                            await SignInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }

                        await UserManager.DeleteAsync(user);

                        foreach (var error in codeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
