using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskBud.Business.Data
{
    public class TaskHistory : EntityBase
    {
        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }


        [Required]
        public string TaskId { get; set; }

        public TaskItem Task { get; set; }


        [Required]
        public DateTimeOffset CreatedOn { get; set; }


        [Required]
        public TaskAction Action { get; set; }


        [Required] 
        public bool IsUndone { get; set; } = false;

        [Required]
        public bool CanUndo { get; set; } = true;

        [Required] 
        public bool CanRedo { get; set; } = true;

        public string OldValueRaw { get; set; }
        public string OldValue { get; set; }

        public string NewValueRaw { get; set; }
        public string NewValue { get; set; }
    }
}
