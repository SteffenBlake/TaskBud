using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBud.Business.Data
{
    public class TaskItem : EntityBase
    {

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public DateTimeOffset CreationDate { get; set; }

        public string AssignedUserId { get; set; }
        public IdentityUser AssignedUser { get; set; }

        [Required]
        public string CreatorId { get; set; }
        public IdentityUser Creator { get; set; }

        public DateTimeOffset? CompletionDate { get; set; }

        public DateTimeOffset? WaitUntil { get; set; }

        public string RepeatCron { get; set; }

        public string StartingAssignedUserId { get; set; }
        public IdentityUser StartingAssignedUser { get; set; }

        [Required]
        public string GroupId { get; set; }

        public TaskGroup Group { get; set; }
    }
}
