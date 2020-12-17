using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Cronos;
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
                TaskAction.Completed => () => UndoComplete(task, history.OldValueRaw),
                TaskAction.Assigned => () => task.AssignedUserId = history.OldValueRaw,
                TaskAction.TitleChange => () => task.Title = history.OldValueRaw,
                TaskAction.DescriptionChange => () => task.Description = history.OldValueRaw,
                TaskAction.PriorityChange => () => task.Priority = Enum.Parse<TaskPriority>(history.OldValueRaw),
                TaskAction.Created => () => throw new NotImplementedException(),
                TaskAction.WaitUntilChange => () => task.WaitUntil = string.IsNullOrEmpty(history.OldValueRaw) ? (DateTimeOffset?)null : DateTimeOffset.Parse(history.OldValueRaw),
                TaskAction.RepeatAfterChange => () => task.RepeatCron = history.OldValueRaw,
                TaskAction.StarterAssignee => () => task.StartingAssignedUserId = history.OldValueRaw,

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

                TaskAction.RepeatAfterChange => 
                    // Only bother sending the signal if "Repeating" <-> "Not Repeating"
                    (string.IsNullOrEmpty(history.OldValueRaw) != string.IsNullOrEmpty(history.NewValueRaw)) ?
                    TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId) :
                    Task.CompletedTask,

                TaskAction.StarterAssignee => Task.CompletedTask,

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
                TaskAction.Completed => () => RedoComplete(task, history.NewValueRaw),
                TaskAction.Assigned => () => task.AssignedUserId = history.NewValueRaw,
                TaskAction.TitleChange => () => task.Title = history.NewValueRaw,
                TaskAction.DescriptionChange => () => task.Description = history.NewValueRaw,
                TaskAction.PriorityChange => () => task.Priority = Enum.Parse<TaskPriority>(history.NewValueRaw),
                TaskAction.Created => () => throw new NotImplementedException(),
                TaskAction.WaitUntilChange => () => task.WaitUntil = string.IsNullOrEmpty(history.NewValueRaw) ? (DateTimeOffset?)null : DateTimeOffset.Parse(history.NewValueRaw),
                TaskAction.RepeatAfterChange => () => task.RepeatCron = history.NewValueRaw,
                TaskAction.StarterAssignee => () => task.StartingAssignedUserId = history.NewValueRaw,
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

                TaskAction.RepeatAfterChange => 
                    // Only bother sending the signal if "Repeating" <-> "Not Repeating"
                    (string.IsNullOrEmpty(history.OldValueRaw) != string.IsNullOrEmpty(history.NewValueRaw)) ?
                    TaskHubContext.Clients.All.SendAsync(TaskHub.Updated, history.TaskId) :
                    Task.CompletedTask,

                TaskAction.StarterAssignee => Task.CompletedTask,

                _ => Task.FromException(new ArgumentOutOfRangeException(nameof(history.Action)))
            });

            return Read(user, historyId);
        }

        private static void UndoComplete(TaskItem task, string oldValueRaw)
        {
            if (string.IsNullOrEmpty(task.RepeatCron))
            {
                task.CompletionDate = null;
            }
            else
            {
                var oldData = JsonSerializer.Deserialize<CompletedTaskRepeatData>(oldValueRaw);
                task.AssignedUserId = oldData.AssignedUserId;
                task.WaitUntil = oldData.WaitUntil;
            }
        }

        private static void RedoComplete(TaskItem task, string newValueRaw)
        {
            if (string.IsNullOrEmpty(task.RepeatCron))
            {
                task.CompletionDate = DateTimeOffset.Now;
            }
            else
            {
                var newData = JsonSerializer.Deserialize<CompletedTaskRepeatData>(newValueRaw);
                task.AssignedUserId = newData.AssignedUserId;
                task.WaitUntil = newData.WaitUntil;
            }
        }

        public async Task CompletedAsync(ClaimsPrincipal user, string taskId)
        {
            var oldValueRaw = "Incomplete";
            var newValueRaw = "Complete";

            var task = await DBContext.FindAsync<TaskItem>(taskId);

            if (!string.IsNullOrEmpty(task.RepeatCron))
            {
                var oldData = new CompletedTaskRepeatData
                {
                    AssignedUserId = task.AssignedUserId,
                    WaitUntil = task.WaitUntil,
                };
                oldValueRaw = JsonSerializer.Serialize(oldData);

                var next = CronExpression.Parse(task.RepeatCron).GetNextOccurrence(DateTime.UtcNow) ?? throw new Exception("Cronos broke, investigate why?");
                var nextLocal = new DateTime(next.Ticks, DateTimeKind.Local);

                var newData = new CompletedTaskRepeatData
                {
                    AssignedUserId = task.StartingAssignedUserId,
                    WaitUntil = nextLocal,
                };
                newValueRaw = JsonSerializer.Serialize(newData);

            }

            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.Completed,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldValueRaw,
                OldValue = "Incomplete",
                NewValueRaw = newValueRaw,
                NewValue = "Complete",
            };

            ResetOldHistories(taskId, TaskAction.Completed);

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

            ResetOldHistories(taskId, TaskAction.Assigned);

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

            ResetOldHistories(taskId, TaskAction.TitleChange);

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

            ResetOldHistories(taskId, TaskAction.DescriptionChange);

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

            ResetOldHistories(taskId, TaskAction.PriorityChange);

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

            ResetOldHistories(taskId, TaskAction.WaitUntilChange);

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        public async Task RepeatAfterChangeAsync(ClaimsPrincipal user, string taskId, string oldRepeatCron, string newRepeatCron)
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
                OldValueRaw = oldRepeatCron,
                OldValue = string.IsNullOrEmpty(oldRepeatCron) ? "" : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(oldRepeatCron),
                NewValueRaw = newRepeatCron,
                NewValue = string.IsNullOrEmpty(newRepeatCron) ? "" : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(newRepeatCron),
            };

            ResetOldHistories(taskId, TaskAction.RepeatAfterChange);
            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }


        public async Task StarterAssigneeChangeAsync(ClaimsPrincipal user, string taskId, string oldUserId, string newUserId)
        {
            var oldUser = string.IsNullOrEmpty(oldUserId) ? null : await UserManager.FindByIdAsync(oldUserId);
            var newUser = string.IsNullOrEmpty(newUserId) ? null : await UserManager.FindByIdAsync(newUserId);

            var history = new TaskHistory
            {
                UserId = user.GetLoggedInUserId<string>(),
                TaskId = taskId,
                CreatedOn = DateTimeOffset.Now,
                Action = TaskAction.StarterAssignee,
                IsUndone = false,
                CanUndo = true,
                CanRedo = true,
                OldValueRaw = oldUserId,
                OldValue = oldUser?.UserName ?? "None",
                NewValueRaw = newUserId,
                NewValue = newUser?.UserName ?? "None"
            };

            ResetOldHistories(taskId, TaskAction.StarterAssignee);

            await DBContext.TaskHistory.AddAsync(history);
            await DBContext.SaveChangesAsync();
        }

        private void ResetOldHistories(string taskId, TaskAction actionType)
        {
            var oldHistories = DBContext.TaskHistory.Where(m => m.Action == actionType && m.TaskId == taskId && m.CanRedo).ToList();
            foreach (var oldHistory in oldHistories)
            {
                oldHistory.CanUndo = false;
                oldHistory.CanRedo = false;
                DBContext.Update(oldHistory);
            }
        }

        public class CompletedTaskRepeatData
        {
            public string AssignedUserId { get; set; }
            public DateTimeOffset? WaitUntil { get; set; }
        }

    }
}
