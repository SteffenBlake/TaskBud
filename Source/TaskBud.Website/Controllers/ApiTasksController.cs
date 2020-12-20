using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;
using TaskBud.Business.Extensions;
using TaskBud.Business.Hubs;
using TaskBud.Business.Models.Tasks;
using TaskBud.Business.Services;

namespace TaskBud.Website.Controllers
{
    [Authorize]
    [Route("api/tasks")]
    public class ApiTasksController : ControllerBase
    {
        private TaskManager TaskManager { get; }

        public ApiTasksController(TaskManager taskManager)
        {
            TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        }

        /// <summary>
        /// Looks up the Task Id of the most recently created task with a specific title
        /// </summary>
        /// <param name="title">The title of the Task to look up and find</param>
        /// <returns>Id of the most recent matching task</returns>
        [HttpGet("lookup")]
        public IActionResult Lookup(string title)
        {
            var id = TaskManager.Lookup(title);
            return Ok(id);
        }

        /// <summary>
        /// Returns a complete Index of all Uncomplete, pending tasks
        /// </summary>
        /// <param name="taskGroupId">(Optional) The Task Group to look up</param>
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index(string taskGroupId)
        {
            var data = await TaskManager.IndexAsync(User, taskGroupId);
            return Ok(data);
        }

        /// <summary>
        /// Reads the data for a single task
        /// </summary>
        /// <param name="taskId">Id of the task to look up.</param>
        [HttpGet("{taskId}")]
        public async Task<IActionResult> Read(string taskId)
        {
            var readData = await TaskManager.ReadAsync(User, taskId);

            return Ok(readData);
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]VMTaskWriteData writeData)
        {
            await TaskManager.CreateAsync(User, writeData);
            return Ok();
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update([FromBody] VMTaskWriteData writeData)
        {
            await TaskManager.UpdateAsync(User, writeData);

            return Ok();
        }

        /// <summary>
        /// Assigns a task to a User by Id, defaults to the authorized user via bearer token
        /// </summary>
        /// <param name="taskId">Id of the Task to assign</param>
        /// <param name="userId">(option) Id of the user to assign to. Defaults to the user associated with the bearer token</param>
        [HttpPost("{taskId}/assign/{userId?}")]
        public async Task<IActionResult> Assign(string taskId, string userId = null)
        { 
            userId ??= User.GetLoggedInUserId<string>();

            await TaskManager.Assign(User, taskId, userId);

            return Ok();
        }

        /// <summary>
        /// Clears the assigned user for a task
        /// </summary>
        /// <param name="taskId">Id of the Task to assign</param>
        /// <returns></returns>
        [HttpPost("{taskId}/un-assign")]
        public async Task<IActionResult> UnAssign(string taskId)
        {
            await TaskManager.UnAssign(User, taskId);

            return Ok();
        }

        /// <summary>
        /// Completes a task.
        /// Normal tasks will be marked as complete.
        /// Repeating tasks will not, and instead have their "Wait until" field updated to match the next available date via the Cron Repeat field,
        /// As well as have their "Assigned User" field be updated to be the "Starter Assigned User" field
        /// </summary>
        /// <param name="taskId">The Id of the task to complete</param>
        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> Complete(string taskId)
        {
            await TaskManager.Complete(User, taskId);

            return Ok();
        }
    }
}
