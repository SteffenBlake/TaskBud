using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.TaskHistories
{
    [ReadOnly(true)]
    public class VMTaskHistory
    {
        public string Id { get; set; }

        public string TaskTitle { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }


        public TaskAction Action { get; set; }

        public bool IsUndone { get; set; }
        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }


        public string OldValueRaw { get; set; }
        public string OldValue { get; set; }

        public string NewValueRaw { get; set; }
        public string NewValue { get; set; }

        public static IQueryable<TaskHistory> Fetch(DbSet<TaskHistory> models)
        {
            return models
                .Include(m => m.User)
                .Include(m => m.Task);
        }

        public static Expression<Func<TaskHistory, VMTaskHistory>> Read(TaskBudDbContext dbContext)
        {
            return model => new VMTaskHistory
            {
                Id = model.Id,
                TaskTitle = model.Task.Title,
                UserName = model.User.UserName,
                CreatedOn = model.CreatedOn,
                Action = model.Action,
                IsUndone = model.IsUndone,
                CanUndo = model.CanUndo,
                CanRedo = model.CanRedo,
                OldValueRaw = model.OldValueRaw,
                OldValue = model.OldValue,
                NewValueRaw = model.NewValueRaw,
                NewValue = model.NewValue,
            };
        }
    }
}
