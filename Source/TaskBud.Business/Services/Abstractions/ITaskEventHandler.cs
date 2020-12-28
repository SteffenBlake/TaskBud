using System.Threading.Tasks;

namespace TaskBud.Business.Services.Abstractions
{
    /// <summary>
    /// Injectable service for firing of Events on Task Modifications
    /// </summary>
    public interface ITaskEventHandler
    {
        /// <summary>
        /// Fire event for a created Task
        /// </summary>
        Task OnCreatedAsync(string taskId);

        /// <summary>
        /// Fire event for an updated Task
        /// </summary>
        Task OnUpdatedAsync(string taskId);

        /// <summary>
        /// Fire event for a completed Task
        /// </summary>
        Task OnCompletedAsync(string taskId);

        /// <summary>
        /// Fire event for a Task Assignment change
        /// </summary>
        Task OnAssignedAsync(string userId, string taskId);

        /// <summary>
        /// Fire event for a created Task being unassigned
        /// </summary>
        Task OnUnAssignedAsync(string previousUserId, string taskId);

        /// <summary>
        /// Fire event for a task being re-opened
        /// </summary>
        Task OnReopened(string userId, string taskId);
    }
}