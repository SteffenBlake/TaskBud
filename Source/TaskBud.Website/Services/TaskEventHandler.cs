using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TaskBud.Business.Services.Abstractions;
using TaskBud.Website.Hubs;

namespace TaskBud.Website.Services
{
    /// <summary>
    /// Non-injectable implementation for <see cref="ITaskEventHandler"/>
    /// For Dependency injection, inject the <see cref="ITaskEventHandler"/>
    /// </summary>
    public class TaskEventHandler : ITaskEventHandler
    {
        private IHubContext<TaskHub> TaskHubContext { get; }

        public TaskEventHandler(IHubContext<TaskHub> taskHubContext)
        {
            TaskHubContext = taskHubContext ?? throw new ArgumentNullException(nameof(taskHubContext));
        }

        /// <inheritdoc/>
        public async Task OnCreatedAsync(string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.Created, taskId);
        }

        /// <inheritdoc/>
        public async Task OnUpdatedAsync(string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, taskId);
        }

        /// <inheritdoc/>
        public async Task OnCompletedAsync(string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.Completed, taskId);
        }

        /// <inheritdoc/>
        public async Task OnAssignedAsync(string userId, string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.Assigned, userId, taskId);
        }

        /// <inheritdoc/>
        public async Task OnUnAssignedAsync(string previousUserId, string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.UnAssigned, previousUserId, taskId);
        }

        /// <inheritdoc/>
        public async Task OnReopened(string userId, string taskId)
        {
            await TaskHubContext.Clients.All.SendAsync(TaskHub.ReOpened, userId, taskId);
        }
    }
}
