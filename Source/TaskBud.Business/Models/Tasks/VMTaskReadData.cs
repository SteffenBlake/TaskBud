using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.Tasks
{
    public class VMTaskReadData : VMTaskWriteData
    {
        [ReadOnly(true)]
        [Display(Name = "Created")]
        public DateTimeOffset CreationDate { get; set; }

        [ReadOnly(true)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Assignee")]
        public string AssignedUser { get; set; }

        [ReadOnly(true)]
        public bool IsAssigned => AssignedUserId != null;

        [ReadOnly(true)]
        public DateTimeOffset? CompletionDate { get; set; }
        public bool Complete => CompletionDate.HasValue;

        [Display(Name = "Starting Assignee")]
        public string StartingAssignedUser { get; set; }

        [ReadOnly(true)]
        public bool HasStarterAssigned => StartingAssignedUserId != null;

        public static IQueryable<TaskItem> Fetch(DbSet<TaskItem> models)
        {
            return models
                .Include(m => m.AssignedUser)
                .Include(m => m.Creator)
                .Include(m => m.Group);
        }

        public static Expression<Func<TaskItem, VMTaskReadData>> Read(TaskBudDbContext dbContext)
        {
            return model => new VMTaskReadData
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                CreationDate = model.CreationDate,
                CompletionDate = model.CompletionDate,
                AssignedUser = model.AssignedUser != null ? model.AssignedUser.UserName : null,
                AssignedUserId = model.AssignedUserId,
                CreatedBy = model.Creator.UserName,
                TaskGroupId = model.Group.Id,
                WaitUntil = model.WaitUntil,
                RepeatCron = model.RepeatCron,
                StartingAssignedUser = model.StartingAssignedUser != null ? model.StartingAssignedUser.UserName : null,
                StartingAssignedUserId = model.StartingAssignedUserId,
            };
        }
    }
}