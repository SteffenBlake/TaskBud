using System;
using System.ComponentModel.DataAnnotations;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.Tasks
{
    public class VMTaskWriteData
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; } = "";

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Display(Name = "Assignee")]
        public string AssignedUserId { get; set; }

        [Required]
        [Display(Name = "Task Group")]
        public string TaskGroupId { get; set; }

        [Display(Name = "Wait Until")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTimeOffset? WaitUntil { get; set; }

        [Display(Name = "Repeat:")]
        [UIHint("CronString")]
        public string RepeatCron { get; set; }

        [Display(Name = "Starting Assignee")]
        public string StartingAssignedUserId { get; set; }

        public void Write(TaskBudDbContext dbContext, TaskItem model)
        {
            model.Title = Title;
            model.Description = Description;
            model.Priority = Priority;
            model.GroupId = TaskGroupId;
            model.AssignedUserId = AssignedUserId;
            model.WaitUntil = WaitUntil;
            model.RepeatCron = RepeatCron;
            model.StartingAssignedUserId = StartingAssignedUserId;
        }
    }
}