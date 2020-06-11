using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Models.TaskGroups;
using TaskBud.Business.Services;

namespace TaskBud.Website.Controllers
{
    [Route("task-groups")]
    [Authorize(Roles ="Administrator")]
    public class TaskGroupsController : Controller
    {
        private TaskGroupManager Manager { get; }
        public TaskGroupsController(TaskGroupManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        [HttpGet]
        [Route("new")]
        public IActionResult New()
        {
            var vm = new VMTaskGroup();
            return View(vm);
        }

        [HttpPost]
        [Route("new")]
        public async Task<IActionResult> New(VMTaskGroup data)
        {
            if (ModelState.IsValid)
            {
                var taskGroupId = await Manager.CreateAsync(data);
                return RedirectToAction("Index", "Tasks", new { taskGroupId });
            }
            else
            {
                return View(data);
            }
        }
    }
}
