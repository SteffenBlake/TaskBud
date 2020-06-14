using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace TaskBud.Website.Controllers
{
    [Route("shutdown")]
    [Authorize(Roles = "Administrator")]
    public class ShutdownController : Controller
    {
        private IHostApplicationLifetime AppLifetime { get; }

        public ShutdownController(IHostApplicationLifetime appLifetime)
        {
            AppLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            AppLifetime.StopApplication();
            return new EmptyResult();
        }
    }
}
