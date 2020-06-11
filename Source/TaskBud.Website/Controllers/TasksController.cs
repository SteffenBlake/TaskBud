using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Extensions;
using TaskBud.Business.Hubs;
using TaskBud.Business.Models.Tasks;
using TaskBud.Business.Services;

namespace TaskBud.Website.Controllers
{
    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private TaskManager TaskManager { get; }

        public TasksController(TaskManager taskManager)
        {
            TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(string taskGroupId)
        {
            var data = await TaskManager.IndexAsync(User, taskGroupId);
            return View(data);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> Read(string taskId)
        {
            var readData = await TaskManager.ReadAsync(User, taskId);

            return PartialView("_TaskItem", readData);
        }

        [HttpGet("create")]
        public IActionResult Create(string taskGroupId)
        {
            var data = new VMTask{ TaskGroupId = taskGroupId };
            return View(data);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(VMTask writeData)
        {
            if (ModelState.IsValid)
            {
                await TaskManager.CreateAsync(User, writeData);

                return RedirectToAction("Index", new { writeData.TaskGroupId });
            }
            else
            {
                return View(writeData);
            }
        }

        [HttpGet("{id}/update")]
        public async Task<IActionResult> Update(string id)
        {
            var data = await TaskManager.ReadAsync(User, id);
            return View(data);
        }

        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(VMTask writeData)
        {
            if (ModelState.IsValid)
            {
                await TaskManager.UpdateAsync(User, writeData);

                return RedirectToAction("Index", new { GroupId = writeData.TaskGroupId });
            }
            else
            {
                return View(writeData);
            }
        }

        [HttpPost("{taskId}/assign/{userId?}")]
        public async Task<IActionResult> Assign(string taskId, string userId)
        { 
            userId ??= User.GetLoggedInUserId<string>();

            await TaskManager.Assign(User, taskId, userId);

            return Ok();
        }

        [HttpPost("{taskId}/un-assign")]
        public async Task<IActionResult> UnAssign(string taskId)
        {
            await TaskManager.UnAssign(User, taskId);

            return Ok();
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> Complete(string taskId)
        {
            await TaskManager.Complete(User, taskId);

            return Ok();
        }
    }
}
