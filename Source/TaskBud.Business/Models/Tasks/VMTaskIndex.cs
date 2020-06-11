using System.Collections.Generic;

namespace TaskBud.Business.Models.Tasks
{
    public class VMTaskIndex
    {
        public IList<VMTask> Tasks { get; set; } = new List<VMTask>();

        public string GroupTitle { get; set; }
        public string GroupId { get; set; }
    }
}
