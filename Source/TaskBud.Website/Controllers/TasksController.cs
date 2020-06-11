using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TaskBud.Business.Models.Tasks;
using TaskBud.Business.Services;
using TaskBud.Website.Extensions;
using TaskBud.Website.Hubs;

namespace TaskBud.Website.Controllers
{
    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private TaskManager TaskManager { get; }
        private IHubContext<TaskHub> TaskHubContext { get; }

        public TasksController(TaskManager taskManager, IHubContext<TaskHub> taskHub)
        {
            TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            TaskHubContext = taskHub ?? throw new ArgumentNullException(nameof(taskHub));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(string taskGroupId)
        {
            var data = await TaskManager.IndexAsync(User.GetLoggedInUserId<string>(), taskGroupId);
            return View(data);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> Read(string taskId)
        {
            var readData = await TaskManager.ReadAsync(taskId);

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
                var readData = await TaskManager.CreateAsync(writeData, User.GetLoggedInUserId<string>());

                await TaskHubContext.Clients.All.SendAsync(TaskHub.Created, readData.Id);

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
            var data = await TaskManager.ReadAsync(id);
            return View(data);
        }

        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(VMTask writeData)
        {
            if (ModelState.IsValid)
            {
                var readData = await TaskManager.UpdateAsync(writeData);

                await TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, readData.Id);

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

            await TaskManager.Assign(taskId, userId);

            await TaskHubContext.Clients.All.SendAsync(TaskHub.Assigned, userId, taskId);

            return Ok();
        }

        [HttpPost("{taskId}/un-assign")]
        public async Task<IActionResult> UnAssign(string taskId)
        {
            var previousUserId = (await TaskManager.ReadAsync(taskId)).AssignedUserId;

            await TaskManager.UnAssign(taskId);

            await TaskHubContext.Clients.All.SendAsync(TaskHub.UnAssigned, previousUserId, taskId);

            return Ok();
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> Complete(string taskId)
        {
            await TaskManager.Complete(taskId);

            await TaskHubContext.Clients.All.SendAsync(TaskHub.Completed, taskId);

            return Ok();
        }

        [HttpPost("{taskId}/re-open")]
        public async Task<IActionResult> ReOpen(string taskId)
        {
            var readData = await TaskManager.ReOpen(taskId);

            await TaskHubContext.Clients.All.SendAsync(TaskHub.ReOpened, readData.AssignedUser, taskId);

            return Ok();
        }
    }
}
