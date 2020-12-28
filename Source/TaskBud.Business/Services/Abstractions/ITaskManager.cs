using System.Security.Claims;
using System.Threading.Tasks;
using TaskBud.Business.Models.Tasks;

namespace TaskBud.Business.Services.Abstractions
{
    /// <summary>
    /// Injectable service for management of Tasks
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Lookups up the newest Task that matches a title
        /// </summary>
        /// <param name="title">The title to match by</param>
        /// <returns>The id of the matched Task</returns>
        string Lookup(string title);

        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="data">Data for creation</param>
        Task<VMTaskReadData> CreateAsync(ClaimsPrincipal user, VMTaskWriteData data);

        /// <summary>
        /// Returns all relevant tasks visible to a user
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="taskGroupId">(Optional) the Task group id to index by</param>
        /// <returns></returns>
        Task<VMTaskIndex> IndexAsync(ClaimsPrincipal user, string taskGroupId = null);

        /// <summary>
        /// Reads the data for a specific task
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="taskId">Id of the task to read from</param>
        Task<VMTaskReadData> ReadAsync(ClaimsPrincipal user, string taskId);

        /// <summary>
        /// Updates a task with new data
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="data">The data to update the task with</param>
        Task<VMTaskReadData> UpdateAsync(ClaimsPrincipal user, VMTaskWriteData data);

        /// <summary>
        /// Completes a task
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="taskId">Id of the task to complete</param>
        Task<VMTaskReadData> Complete(ClaimsPrincipal user, string taskId);

        /// <summary>
        /// Assigns a task to a User
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="taskId">Id of the task to assign</param>
        /// <param name="userId">Id of the user to assign the task to</param>
        Task<VMTaskReadData> Assign(ClaimsPrincipal user, string taskId, string userId);

        /// <summary>
        /// Sets a tasks Assignee to "none" 
        /// </summary>
        /// <param name="user">Active user performing the action</param>
        /// <param name="taskId">The id of the task to Un-assign</param>
        /// <returns></returns>
        Task<VMTaskReadData> UnAssign(ClaimsPrincipal user, string taskId);
    }
}