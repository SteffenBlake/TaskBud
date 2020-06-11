using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBud.Business.Data;
using TaskBud.Business.Models.TaskGroups;

namespace TaskBud.Business.Services
{
    public class TaskGroupManager
    {
        private TaskBudDbContext DBContext { get; }

        public TaskGroupManager(TaskBudDbContext dbContext)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<string> CreateAsync(VMTaskGroup data)
        {
            var entity = new TaskGroup();
            data.Write(DBContext, entity);

            await DBContext.TaskGroups.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            return entity.Id;
        }

        public Task<IList<VMTaskGroup>> IndexAsync()
        {
            IList<VMTaskGroup> data = DBContext.TaskGroups
                .Select(VMTaskGroup.Read(DBContext)).ToList();

            return Task.FromResult(data);
        }

        public async Task UpdateAsync(VMTaskGroup data)
        {
            var entity = await DBContext.TaskGroups.FindAsync(data.Id);

            data.Write(DBContext, entity);
            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();
        }
    }
}
