using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskBud.Business.Data
{
    public class TaskGroup : EntityBase
    {
        [Required]
        public string Title { get; set; }

        public IList<TaskItem> Tasks { get; set; }
    }
}