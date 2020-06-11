using System;
using System.Linq.Expressions;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.TaskGroups
{
    public class VMTaskGroup
    {
        public string Id { get; set; }

        public string Title { get; set; }


        public static Expression<Func<TaskGroup, VMTaskGroup>> Read(TaskBudDbContext db)
        {
            return 
                (model) =>
                    new VMTaskGroup
                    {
                        Id = model.Id,
                        Title = model.Title
                    };
        }

        public void Write(TaskBudDbContext dbContext, TaskGroup model)
        {
            model.Title = Title;
        }

    }
}
