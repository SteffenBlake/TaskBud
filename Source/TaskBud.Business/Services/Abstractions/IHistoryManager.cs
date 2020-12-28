using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskBud.Business.Data;
using TaskBud.Business.Models.TaskHistories;

namespace TaskBud.Business.Services.Abstractions
{
    public interface IHistoryManager
    {
        /// <summary>
        /// Returns all History Entries for a given user
        /// </summary>
        /// <param name="user">The user to fetch entities for</param>
        /// <param name="limit">The max number of entries to fetch</param>
        /// <returns></returns>
        VMTaskHistoryIndex Index(ClaimsPrincipal user, int limit);

        /// <summary>
        /// Reads the data for a specific history entry for a user
        /// </summary>
        /// <param name="user">The user to fetch the history data for</param>
        /// <param name="historyId">The id of the specific history entry to read</param>
        /// <returns></returns>
        VMTaskHistory Read(ClaimsPrincipal user, string historyId);

        /// <summary>
        /// Performs the Undo operation for a history entry
        /// </summary>
        /// <param name="user">The User who performed the Undo</param>
        /// <param name="historyId">The Id of the history Entry</param>
        Task<VMTaskHistory> UndoAsync(ClaimsPrincipal user, string historyId);

        /// <summary>
        /// Performs the Redo operation for a history entry
        /// </summary>
        /// <param name="user">The User who performed the Redo</param>
        /// <param name="historyId">The Id of the history Entry</param>
        Task<VMTaskHistory> RedoAsync(ClaimsPrincipal user, string historyId);

        /// <summary>
        /// Composes a "Completed" history entry for a Task
        /// </summary>
        /// <param name="user">The user who completed the Task</param>
        /// <param name="taskId">The Id of the Task</param>
        Task CompletedAsync(ClaimsPrincipal user, string taskId);

        /// <summary>
        /// Composes an "Assigned" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who performed the assignment</param>
        /// <param name="taskId">The Id of the task</param>
        /// <param name="oldUserId">The original Id of the Assigned User for the task</param>
        /// <param name="newUserId">The id of the new Assignee</param>
        Task AssignedAsync(ClaimsPrincipal user, string taskId, string oldUserId, string newUserId);

        /// <summary>
        /// Composes a "Title changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the title</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldTitle">The title before the change</param>
        /// <param name="newTitle">The new title after the change</param>
        Task TitleChangeAsync(ClaimsPrincipal user, string taskId, string oldTitle, string newTitle);

        /// <summary>
        /// Composes a "Description changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the title</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldDescription">The description before the change</param>
        /// <param name="newDescription">The new description after the change</param>
        Task DescriptionChangeAsync(ClaimsPrincipal user, string taskId, string oldDescription, string newDescription);

        /// <summary>
        /// Composes a "Priority changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the title</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldPriority">The priority before the change</param>
        /// <param name="newPriority">The new priority after the change</param>
        Task PriorityChangeAsync(ClaimsPrincipal user, string taskId, TaskPriority oldPriority, TaskPriority newPriority);

        /// <summary>
        /// Composes a "Created" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who created the Task</param>
        /// <param name="taskId">The id of the new Task</param>
        /// <param name="taskTitle">The Title of the Task</param>
        Task CreatedAsync(ClaimsPrincipal user, string taskId, string taskTitle);

        /// <summary>
        /// Composes a "Wait Until changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the Wait Until date</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldWaitUntil">The Wait Until date before the change</param>
        /// <param name="newWaitUntil">The new Wait Until date after the change</param>
        Task WaitUntilChangeAsync(ClaimsPrincipal user, string taskId, DateTimeOffset? oldWaitUntil, DateTimeOffset? newWaitUntil);

        /// <summary>
        /// Composes a "Repeat After changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the Repeat Cron</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldRepeatCron">The Repeat Cron before the change</param>
        /// <param name="newRepeatCron">The new Repeat Cron after the change</param>
        Task RepeatAfterChangeAsync(ClaimsPrincipal user, string taskId, string oldRepeatCron, string newRepeatCron);

        /// <summary>
        /// Composes a "Starter Assignee changed" history entry for a Task
        /// </summary>
        /// <param name="user">The active user who changed the Starter Assignee Id</param>
        /// <param name="taskId">The id of the task</param>
        /// <param name="oldUserId">The Starter Assignee Id before the change</param>
        /// <param name="newUserId">The new Starter Assignee Id after the change</param>
        Task StarterAssigneeChangeAsync(ClaimsPrincipal user, string taskId, string oldUserId, string newUserId);
    }
}