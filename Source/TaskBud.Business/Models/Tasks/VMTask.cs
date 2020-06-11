using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.Tasks
{
    public class VMTask
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; } = "";

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [ReadOnly(true)]
        public DateTimeOffset CreationDate { get; set; }

        [ReadOnly(true)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Assignee")]
        public string AssignedUser { get; set; }
        public string AssignedUserId { get; set; }

        [ReadOnly(true)]
        public bool IsAssigned => AssignedUserId != null;


        [ReadOnly(true)]
        public DateTimeOffset? CompletionDate { get; set; }
        public bool Complete => CompletionDate.HasValue;

        [Required]
        public string TaskGroupId { get; set; }

        public static IQueryable<TaskItem> Fetch(DbSet<TaskItem> models)
        {
            return models
                .Include(m => m.AssignedUser)
                .Include(m => m.Creator)
                .Include(m => m.Group);
        }

        public static Expression<Func<TaskItem, VMTask>> Read(TaskBudDbContext dbContext)
        {
            return model => new VMTask
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
                TaskGroupId = model.Group.Id
            };
        }

        public async Task WriteAsync(TaskBudDbContext dbContext, TaskItem model)
        {
            model.Title = Title;
            model.Description = Description;
            model.Priority = Priority;
            model.Group = await dbContext.TaskGroups.FindAsync(TaskGroupId);
            model.AssignedUserId = AssignedUserId;
        }
    }
}