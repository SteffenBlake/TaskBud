using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Extensions;
using TaskBud.Business.Models.Tasks;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Website.Controllers
{
    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private ITaskManager TaskManager { get; }

        public TasksController(ITaskManager taskManager)
        {
            TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        }

        [HttpGet("")]
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
            var data = new VMTaskReadData{ TaskGroupId = taskGroupId };
            return View(data);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(VMTaskReadData writeData)
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
        public async Task<IActionResult> Update(VMTaskReadData writeData)
        {
            if (ModelState.IsValid)
            {
                await TaskManager.UpdateAsync(User, writeData);

                return RedirectToAction("Index", new { writeData.TaskGroupId });
            }
            else
            {
                return View(writeData);
            }
        }

        [HttpPost("{taskId}/assign/{userId?}")]
        public async Task<IActionResult> Assign(string taskId, string userId = null)
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
