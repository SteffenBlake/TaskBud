using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBud.Business.Data;
using TaskBud.Business.Models.TaskGroups;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Business.Services.Implementations
{
    /// <summary>
    /// Non-injectable implementation for <see cref="ITaskGroupManager"/>
    /// For Dependency injection, inject the <see cref="ITaskGroupManager"/>
    /// </summary>
    public class TaskGroupManager : ITaskGroupManager
    {
        private TaskBudDbContext DBContext { get; }

        public TaskGroupManager(TaskBudDbContext dbContext)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <inheritdoc/>
        public async Task<string> CreateAsync(VMTaskGroup data)
        {
            var entity = new TaskGroup();
            data.Write(DBContext, entity);

            await DBContext.TaskGroups.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc/>
        public Task<IList<VMTaskGroup>> IndexAsync()
        {
            IList<VMTaskGroup> data = DBContext.TaskGroups
                .Select(VMTaskGroup.Read(DBContext)).ToList();

            return Task.FromResult(data);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(VMTaskGroup data)
        {
            var entity = await DBContext.TaskGroups.FindAsync(data.Id);

            data.Write(DBContext, entity);
            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();
        }
    }
}
