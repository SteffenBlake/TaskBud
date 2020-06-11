using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskBud.Business.Services;

namespace TaskBud.Website.Controllers
{
    [Route("invitations")]
    [Authorize(Roles ="Administrator")]
    public class InvitationsController : Controller
    {
        private InvitationManager InvitationManager { get; }
        public InvitationsController(InvitationManager invitationManager)
        {
            InvitationManager = invitationManager ?? throw new System.ArgumentNullException(nameof(invitationManager));
        }

        // GET: ~/invitations
        // GET: ~/invitations/index
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var vm = await InvitationManager.IndexAsync();
            return View(vm);
        }

        // POST: ~/invitations/new
        [HttpPost("new")]
        public async Task<IActionResult> New()
        {
            var data = await InvitationManager.CreateAsync();
            return PartialView("DisplayTemplates/VMInvitation", data);
        }

        // POST: ~/invitations/{code}/disable
        [HttpPost("{code}/expire")]
        public async Task<IActionResult> Expire(string code)
        {
            var data = await InvitationManager.ExpireAsync(code);
            return PartialView("DisplayTemplates/VMInvitation", data);
        }
    }
}
