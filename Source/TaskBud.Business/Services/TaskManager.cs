using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;
using TaskBud.Business.Extensions;
using TaskBud.Business.Hubs;
using TaskBud.Business.Models.Tasks;

namespace TaskBud.Business.Services
{
    public class TaskManager
    {
        private TaskBudDbContext DBContext { get; }
        private IHubContext<TaskHub> TaskHubContext { get; }
        private HistoryManager HistoryManager { get; }

        public TaskManager(TaskBudDbContext dbContext, IHubContext<TaskHub> taskHubContext, HistoryManager historyManager)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            TaskHubContext = taskHubContext ?? throw new ArgumentNullException(nameof(taskHubContext));
            HistoryManager = historyManager;
        }

        public async Task<VMTask> CreateAsync(ClaimsPrincipal user, VMTask data)
        {
            var entity = new TaskItem(); 
            data.Write(DBContext, entity);

            entity.CreatorId = user.GetLoggedInUserId<string>();
            entity.CreationDate = DateTimeOffset.Now;

            await DBContext.TaskItems.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            await HistoryManager.CreatedAsync(user, entity.Id, data.Title);

            await TaskHubContext.Clients.All.SendAsync(TaskHub.Created, data.Id);

            return await ReadAsync(user, entity.Id);
        }

        public async Task<VMTaskIndex> IndexAsync(ClaimsPrincipal user, string taskGroupId = null)
        {
            var userId = user.GetLoggedInUserId<string>();

            var data = new VMTaskIndex();

            var tasks = VMTask.Fetch(DBContext.TaskItems);

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

            data.Tasks = tasks.Select(VMTask.Read(DBContext)).ToList();

            return data;
        }

        public async Task<VMTask> ReadAsync(ClaimsPrincipal user, string taskId)
        {
            var entity = await 
                VMTask.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == taskId);
            return VMTask.Read(DBContext).Compile().Invoke(entity);
        }

        public async Task<VMTask> UpdateAsync(ClaimsPrincipal user, VMTask data)
        {
            var entity = await 
                VMTask.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == data.Id);

            // To keep tracking simple, we revert RepeatAfterType back to "Days" if the count is left blank
            // This way, changing the "RepeatAfterType" property and leaving "Count" blank wont trigger events
            if (data.RepeatAfterCount == null)
            {
                data.RepeatAfterType = RepeatType.Days;
            }

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
                              
            if (entity.RepeatAfterCount != data.RepeatAfterCount || entity.RepeatAfterType != data.RepeatAfterType)
            {
                await HistoryManager.RepeatAfterChangeAsync(user, data.Id, (entity.RepeatAfterCount, entity.RepeatAfterType), (data.RepeatAfterCount, data.RepeatAfterType));
            }

            data.Write(DBContext, entity);

            DBContext.Update(entity);

            await DBContext.SaveChangesAsync();

            await TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, data.Id);

            return await ReadAsync(user, entity.Id);
        }

        public async Task<VMTask> Complete(ClaimsPrincipal user, string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.CompletionDate = DateTimeOffset.Now;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await HistoryManager.CompletedAsync(user, taskId);
            await TaskHubContext.Clients.All.SendAsync(TaskHub.Completed, taskId);

            var readData = await ReadAsync(user, entity.Id);

            if (entity.RepeatAfterCount != null)
            {
                var newTask = new TaskItem();
                readData.Write(DBContext, newTask);
                newTask.Id = Guid.NewGuid().ToString();
                newTask.CreationDate = DateTimeOffset.Now;
                newTask.CompletionDate = null;
                newTask.WaitUntil = entity.RepeatAfterType switch
                {
                    RepeatType.Minutes => DateTimeOffset.Now.AddMinutes(entity.RepeatAfterCount.Value),
                    RepeatType.Hours => DateTimeOffset.Now.AddHours(entity.RepeatAfterCount.Value),
                    RepeatType.Days => DateTimeOffset.Now.AddDays(entity.RepeatAfterCount.Value),
                    RepeatType.Weeks => DateTimeOffset.Now.AddDays(entity.RepeatAfterCount.Value * 7),
                    RepeatType.Months => DateTimeOffset.Now.AddMonths(entity.RepeatAfterCount.Value),
                    RepeatType.Years => DateTimeOffset.Now.AddYears(entity.RepeatAfterCount.Value),
                    _ => throw new ArgumentOutOfRangeException(nameof(entity.RepeatAfterType))
                };

                await DBContext.TaskItems.AddAsync(newTask);
                await DBContext.SaveChangesAsync();
            }


            return readData;
        }

        public async Task<VMTask> Assign(ClaimsPrincipal user, string taskId, string userId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.AssignedUserId = userId;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await TaskHubContext.Clients.All.SendAsync(TaskHub.Assigned, userId, taskId);

            return await ReadAsync(user, entity.Id);
        }

        public async Task<VMTask> UnAssign(ClaimsPrincipal user, string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);

            var previousUserId = entity.AssignedUserId;
            entity.AssignedUserId = null;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            await TaskHubContext.Clients.All.SendAsync(TaskHub.UnAssigned, previousUserId, taskId);

            return await ReadAsync(user, entity.Id);
        }
    }

}
