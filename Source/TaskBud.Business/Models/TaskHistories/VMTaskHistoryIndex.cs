using System.Collections.Generic;

namespace TaskBud.Business.Models.TaskHistories
{
    public class VMTaskHistoryIndex
    {
        public IList<VMTaskHistory> Items { get; set; }

        public int Limit { get; set; }
    }
}