using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Cronos;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;
using TaskBud.Business.Extensions;
using TaskBud.Business.Models.Tasks;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Business.Services.Implementations
{
    /// <summary>
    /// Non-injectable implementation for <see cref="ITaskManager"/>
    /// For Dependency injection, inject the <see cref="ITaskManager"/>
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private TaskBudDbContext DBContext { get; }
        private IHistoryManager HistoryManager { get; }
        private ITaskEventHandler TaskEventHandler { get; }

        public TaskManager(TaskBudDbContext dbContext, IHistoryManager historyManager, ITaskEventHandler taskEventHandler)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            HistoryManager = historyManager ?? throw new ArgumentNullException(nameof(historyManager));
            TaskEventHandler = taskEventHandler ?? throw new ArgumentNullException(nameof(taskEventHandler));
        }

        /// <inheritdoc/>
        public string Lookup(string title)
        {
            return DBContext.TaskItems.OrderByDescending(t => t.CreationDate).First(t => t.Title == title).Id;
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> CreateAsync(ClaimsPrincipal user, VMTaskWriteData data)
        {
            var entity = new TaskItem(); 
            data.Write(DBContext, entity);

            entity.CreatorId = user.GetLoggedInUserId<string>();
            entity.CreationDate = DateTimeOffset.Now;

            await DBContext.TaskItems.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            await HistoryManager.CreatedAsync(user, entity.Id, data.Title);

            await TaskEventHandler.OnCreatedAsync(entity.Id);

            return await ReadAsync(user, entity.Id);
        }

        /// <inheritdoc/>
        public async Task<VMTaskIndex> IndexAsync(ClaimsPrincipal user, string taskGroupId = null)
        {
            var userId = user.GetLoggedInUserId<string>();

            var data = new VMTaskIndex();

            var tasks = VMTaskReadData.Fetch(DBContext.TaskItems);

            if (taskGroupId != null)
            {
                tasks = tasks.Where(m => m.Group.Id == taskGroupId);
                var group = await DBContext.TaskGroups.FindAsync(taskGroupId);
                data.GroupId = group.Id;
                data.GroupTitle = group.Title;
            }
            else
            {
                data.GroupTitle = "All";
            }

            tasks = tasks
                .Where(t => t.AssignedUserId == null || t.AssignedUserId == userId)
                .Where(t => t.CompletionDate == null)
                .Where(t => t.WaitUntil == null || DateTime.Now > t.WaitUntil);

            tasks = tasks.OrderByDescending(t => t.Priority);

            data.Tasks = tasks.Select(VMTaskReadData.Read(DBContext)).ToList();

            return data;
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> ReadAsync(ClaimsPrincipal user, string taskId)
        {
            var entity = await 
                VMTaskReadData.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == taskId);
            return VMTaskReadData.Read(DBContext).Compile().Invoke(entity);
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> UpdateAsync(ClaimsPrincipal user, VMTaskWriteData data)
        {
            var entity = await 
                VMTaskReadData.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == data.Id);

            if (entity.AssignedUserId != data.AssignedUserId)
            {
                await HistoryManager.AssignedAsync(user, data.Id, entity.AssignedUserId, data.AssignedUserId);
            }

            if (entity.Priority != data.Priority)
            {
                await HistoryManager.PriorityChangeAsync(user, data.Id, entity.Priority, data.Priority);
            }

            if (entity.Title != data.Title)
            {
                await HistoryManager.TitleChangeAsync(user, data.Id, entity.Title, data.Title);
            }

            if (entity.Description != data.Description)
            {
                await HistoryManager.DescriptionChangeAsync(user, data.Id, entity.Description, data.Description);
            }

            if (entity.WaitUntil != data.WaitUntil)
            {
                await HistoryManager.WaitUntilChangeAsync(user, data.Id, entity.WaitUntil, data.WaitUntil);
            }
                              
            if (entity.RepeatCron != data.RepeatCron)
            {
                await HistoryManager.RepeatAfterChangeAsync(user, data.Id, entity.RepeatCron, data.RepeatCron);
            }

            if (entity.StartingAssignedUserId != data.StartingAssignedUserId)
            {
                await HistoryManager.StarterAssigneeChangeAsync(user, data.Id, entity.StartingAssignedUserId, data.StartingAssignedUserId);
            }

            data.Write(DBContext, entity);

            DBContext.Update(entity);

            await DBContext.SaveChangesAsync();

            await TaskEventHandler.OnUpdatedAsync(entity.Id);

            return await ReadAsync(user, entity.Id);
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> Complete(ClaimsPrincipal user, string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);

            // Because Cronos cannot provide a "LastOccurrence" we need to execute the HistoryManager first
            await HistoryManager.CompletedAsync(user, taskId);

            if (!string.IsNullOrEmpty(entity.RepeatCron))
            {
                var next = CronExpression.Parse(entity.RepeatCron).GetNextOccurrence(DateTime.UtcNow) ?? throw new Exception("Cronos broke, investigate why?");
                var nextLocal = new DateTime(next.Ticks, DateTimeKind.Local);
                entity.WaitUntil = new DateTimeOffset(nextLocal);
                entity.AssignedUserId = entity.StartingAssignedUserId;
            }
            else
            {
                entity.CompletionDate = DateTimeOffset.Now;
            }

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await TaskEventHandler.OnCompletedAsync(taskId);
            var readData = await ReadAsync(user, entity.Id);

            return readData;
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> Assign(ClaimsPrincipal user, string taskId, string userId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.AssignedUserId = userId;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await TaskEventHandler.OnAssignedAsync(userId, taskId);

            return await ReadAsync(user, entity.Id);
        }

        /// <inheritdoc/>
        public async Task<VMTaskReadData> UnAssign(ClaimsPrincipal user, string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);

            var previousUserId = entity.AssignedUserId;
            entity.AssignedUserId = null;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await TaskEventHandler.OnUnAssignedAsync(previousUserId, taskId);

            return await ReadAsync(user, entity.Id);
        }
    }

}
