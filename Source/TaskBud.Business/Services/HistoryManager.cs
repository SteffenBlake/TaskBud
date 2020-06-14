using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using TaskBud.Business.Data;
using TaskBud.Business.Extensions;
using TaskBud.Business.Hubs;
using TaskBud.Business.Models.TaskHistories;

namespace TaskBud.Business.Services
{
    public class HistoryManager
    {
        private TaskBudDbContext DBContext { get; }
        private UserManager<IdentityUser> UserManager { get; }
        private IHubContext<TaskHub> TaskHubContext { get; }

        public HistoryManager(TaskBudDbContext dbContext, UserManager<IdentityUser> userManager, IHubContext<TaskHub> taskHub)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            TaskHubContext = taskHub ?? throw new ArgumentNullException(nameof(taskHub));
        }

        public VMTaskHistoryIndex Index(ClaimsPrincipal user, int limit)
        {
            var items = VMTaskHistory.Fetch(DBContext.TaskHistory)
                .Where(m => m.UserId == user.GetLoggedInUserId<string>() && m.CanRedo)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .Select(VMTaskHistory.Read(DBContext))
                .ToList();
            
            var data = new VMTaskHistoryIndex
            {
                Items = items,
                Limit = limit
            };

            return data;
        }

        public VMTaskHistory Read(ClaimsPrincipal user, string historyId)
        {
            var data = VMTaskHistory.Fetch(DBContext.TaskHistory)
                .Where(m => m.UserId == user.GetLoggedInUserId<string>() && m.CanRedo && m.Id == historyId)
                .Select(VMTaskHistory.Read(DBContext))
                .Single();

            return data;
        }

        public async Task<VMTaskHistory> UndoAsync(ClaimsPrincipal user, string historyId)
        {
            var userId = user.GetLoggedInUserId<string>();
            var history = DBContext.TaskHistory.Single(m => m.UserId == userId && !m.IsUndone && m.CanUndo && m.Id == historyId);

            var task = await DBContext.TaskItems.FindAsync(history.TaskId);

            (history.Action switch
            {
                TaskAction.Completed => () => task.CompletionDate = null,
                TaskAction.Assigned => () => task.AssignedUserId = history.OldValueRaw,
                TaskAction.TitleChange => () => task.Title = history.OldValueRaw,
                TaskAction.DescriptionChange => () => task.Description = history.OldValueRaw,
                TaskAction.PriorityChange => () => task.Priority = Enum.Parse<TaskPriority>(history.OldValueRaw),
                TaskAction.Created => () => throw new NotImplementedException(),
                TaskAction.WaitUntilChange => () => task.WaitUntil = string.IsNullOrEmpty(history.OldValueRaw) ? (DateTimeOffset?)null : DateTimeOffset.Parse(history.OldValueRaw),
                TaskAction.RepeatAfterChange => () =>
                {
                    var data = JsonSerializer.Deserialize<RepeatContainer>(history.OldValueRaw);
                    task.RepeatAfterCount = data.RepeatAfterCount;
                    task.RepeatAfterType = data.RepeatAfterType;
                },

                _ => (Action)(() => throw new ArgumentOutOfRangeException(nameof(history.Action)))
            })();

            history.IsUndone = true;
            history.CanUndo = false;

            DBContext.Update(history);
            DBContext.Update(task);
            await DBContext.SaveChangesAsync();

            await (history.Action switch
            {
                TaskAction.Completed => TaskHubContext.Clients.All.SendAsync(TaskHub.ReOpened, userId, history.TaskId),

                TaskAction.Assigned => 
                // Undone: Not Null => Null = Unassigned
                string.IsNullOrEmpty(history.OldValueRaw) ?
                    TaskHubContext.Clients.All.SendAsync(TaskHub.UnAssigned, history.NewValueRaw, history.TaskId) :
                // Undone: Null or Not Null => Not Null = Assigned
                    TaskHubContext.Clients.All.SendAsync(TaskHub.Assigned, userId, history.TaskId),

                TaskAction.TitleChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.DescriptionChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.PriorityChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.Created => Task.FromException(new NotImplementedException()),

                TaskAction.WaitUntilChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.RepeatAfterChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                _ => Task.FromException(new ArgumentOutOfRangeException(nameof(history.Action)))
            });

            return Read(user, historyId);
        }

        public async Task<VMTaskHistory> RedoAsync(ClaimsPrincipal user, string historyId)
        {
            var userId = user.GetLoggedInUserId<string>();
            var history = DBContext.TaskHistory.Single(m => m.UserId == userId && m.IsUndone && m.CanRedo && m.Id == historyId);

            if (!history.IsUndone || !history.CanRedo)
            {
                throw new InvalidOperationException();
            }

            var task = await DBContext.TaskItems.FindAsync(history.TaskId);

            (history.Action switch
            {
                TaskAction.Completed => () => task.CompletionDate = DateTimeOffset.Now,
                TaskAction.Assigned => () => task.AssignedUserId = history.NewValueRaw,
                TaskAction.TitleChange => () => task.Title = history.NewValueRaw,
                TaskAction.DescriptionChange => () => task.Description = history.NewValueRaw,
                TaskAction.PriorityChange => () => task.Priority = Enum.Parse<TaskPriority>(history.NewValueRaw),
                TaskAction.Created => () => throw new NotImplementedException(),
                TaskAction.WaitUntilChange => () => task.WaitUntil = string.IsNullOrEmpty(history.NewValueRaw) ? (DateTimeOffset?)null : DateTimeOffset.Parse(history.NewValueRaw),

                TaskAction.RepeatAfterChange => () =>
                {

                    var data = JsonSerializer.Deserialize<RepeatContainer>(history.OldValueRaw);
                    task.RepeatAfterCount = data.RepeatAfterCount;
                    task.RepeatAfterType = data.RepeatAfterType;
                },

                _ => (Action)(() => throw new ArgumentOutOfRangeException(nameof(history.Action)))
            })();

            history.IsUndone = false;
            history.CanUndo = true;

            DBContext.Update(history);
            DBContext.Update(task);
            await DBContext.SaveChangesAsync();

            await (history.Action switch
            {
                TaskAction.Completed => TaskHubContext.Clients.All.SendAsync(TaskHub.Completed, history.TaskId),

                TaskAction.Assigned =>
                // Redone: Not Null => Null = Unassigned
                string.IsNullOrEmpty(history.NewValueRaw) ?
                    TaskHubContext.Clients.All.SendAsync(TaskHub.UnAssigned, history.OldValueRaw, history.TaskId) : 
                // Redone: Null or Not Null => Not Null = Assigned
                    TaskHubContext.Clients.All.SendAsync(TaskHub.Assigned, userId, history.TaskId),

                TaskAction.TitleChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.DescriptionChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.PriorityChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.Created => Task.FromException(new NotImplementedException()),

                TaskAction.WaitUntilChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),

                TaskAction.RepeatAfterChange => TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId),


                _ => Task.FromException(new ArgumentOutOfRangeException(nameof(history.Action)))
            });

            return Read(user, historyId);
        }

        public async Task CompletedAsync(ClaimsPrincipal user, string taskId)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.Completed,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = "Incomplete",
                OldValue = "Incomplete",
                NewValueRaw = "Complete",
                NewValue = "Complete",
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.Completed && m.TaskId == taskId && m.CanRedo).ToList();
            foreach(var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task AssignedAsync(ClaimsPrincipal user, string taskId, string oldUserId, string newUserId)
        {
            var oldUser = string.IsNullOrEmpty(oldUserId) ? null : await UserManager.FindByIdAsync(oldUserId);
            var newUser = string.IsNullOrEmpty(newUserId) ? null : await UserManager.FindByIdAsync(newUserId);

            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.Assigned,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldUserId,
                OldValue = oldUser?.UserName ?? "None",
                NewValueRaw = newUserId,
                NewValue = newUser?.UserName ?? "None"
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.Assigned && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task TitleChangeAsync(ClaimsPrincipal user, string taskId, string oldTitle, string newTitle)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.TitleChange,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldTitle,
                OldValue = oldTitle,
                NewValueRaw = newTitle,
                NewValue = newTitle
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.TitleChange && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task DescriptionChangeAsync(ClaimsPrincipal user, string taskId, string oldDescription, string newDescription)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.DescriptionChange,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldDescription,
                OldValue = oldDescription,
                NewValueRaw = newDescription,
                NewValue = newDescription
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.DescriptionChange && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task PriorityChangeAsync(ClaimsPrincipal user, string taskId, TaskPriority oldPriority, TaskPriority newPriority)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.PriorityChange,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldPriority.ToString("G"),
                OldValue = oldPriority.GetDisplayName(),
                NewValueRaw = newPriority.ToString("G"),
                NewValue = newPriority.GetDisplayName()
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.PriorityChange && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task CreatedAsync(ClaimsPrincipal user, string taskId, string taskTitle)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.Created,
                IsUndone = false,
                CanUndo = false,
                CanRedo = false,
                OldValueRaw = "",
                OldValue = "",
                NewValueRaw = taskId,
                NewValue = taskTitle
            };

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task WaitUntilChangeAsync(ClaimsPrincipal user, string taskId, DateTimeOffset? oldWaitUntil, DateTimeOffset? newWaitUntil)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.WaitUntilChange,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldWaitUntil?.ToString("O"),
                OldValue = oldWaitUntil?.ToHumanReadable(),
                NewValueRaw = newWaitUntil?.ToString("O"),
                NewValue = newWaitUntil?.ToHumanReadable()
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.WaitUntilChange && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task RepeatAfterChangeAsync(ClaimsPrincipal user, string taskId, (int? Count, RepeatType Type) oldRepeat, (int? Count, RepeatType Type) newRepeat)
        {
            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.RepeatAfterChange,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = JsonSerializer.Serialize(new RepeatContainer(oldRepeat.Count, oldRepeat.Type)),
                OldValue = $"{oldRepeat.Count} {oldRepeat.Type.GetDisplayName()}",
                NewValueRaw = JsonSerializer.Serialize(new RepeatContainer(newRepeat.Count, newRepeat.Type)),
                NewValue = $"{newRepeat.Count} {newRepeat.Type.GetDisplayName()}"
            };

            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == TaskAction.RepeatAfterChange && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public class RepeatContainer
        {
            public RepeatContainer() {}

            public RepeatContainer(int? repeatAfterCount, RepeatType repeatAfterType)
            {
                RepeatAfterCount = repeatAfterCount;
                RepeatAfterType = repeatAfterType;
            }

            public int? RepeatAfterCount { get; set; }
            public RepeatType RepeatAfterType { get; set; }
        }
    }
}
