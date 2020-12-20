using System.Collections.Generic;

namespace TaskBud.Business.Models.Tasks
{
    public class VMTaskIndex
    {
        public IList<VMTaskReadData> Tasks { get; set; } = new List<VMTaskReadData>();

        public string GroupTitle { get; set; }
        public string GroupId { get; set; }
    }
}
