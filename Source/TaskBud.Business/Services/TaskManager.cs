using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskBud.Business.Data;
using TaskBud.Business.Models.Tasks;

namespace TaskBud.Business.Services
{
    public class TaskManager
    {
        private TaskBudDbContext DBContext { get; }
        private UserManager<IdentityUser> UserManager { get; }

        public TaskManager(TaskBudDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<VMTask> CreateAsync(VMTask data, string activeUserId)
        {
            var userTask = UserManager.FindByIdAsync(activeUserId);

            var entity = new TaskItem();
            await data.WriteAsync(DBContext, entity);

            entity.Creator = await userTask;
            entity.CreationDate = DateTimeOffset.Now;

            await DBContext.TaskItems.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }

        public async Task<VMTaskIndex> IndexAsync(string userId, string taskGroupId = null)
        {
            var data = new VMTaskIndex();

            var tasks = VMTask.Fetch(DBContext.TaskItems);

            if (taskGroupId != null)
            {
                tasks = tasks.Where(m => m.Group.Id == taskGroupId);
                var group = await DBContext.TaskGroups.FindAsync(taskGroupId);
                data.GroupId = group.Id;
                data.GroupTitle = group.Title;
            }
            else
            {
                data.GroupTitle = "All";
            }

            tasks = tasks
                .Where(t => t.AssignedUserId == null || t.AssignedUserId == userId)
                .Where(t => t.CompletionDate == null);

            tasks = tasks.OrderByDescending(t => t.Priority);

            data.Tasks = tasks.Select(VMTask.Read(DBContext)).ToList();

            return data;
        }

        public async Task<VMTask> ReadAsync(string taskId)
        {
            var entity = await 
                VMTask.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == taskId);
            return VMTask.Read(DBContext).Compile().Invoke(entity);
        }

        public async Task<VMTask> UpdateAsync(VMTask data)
        {
            var entity = await 
                VMTask.Fetch(DBContext.TaskItems)
                .SingleAsync(m => m.Id == data.Id);

            await data.WriteAsync(DBContext, entity);

            DBContext.Update(entity);

            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }

        public async Task<VMTask> Complete(string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.CompletionDate = DateTimeOffset.Now;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }

        public async Task<VMTask> ReOpen(string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.CompletionDate = null;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }

        public async Task<VMTask> Assign(string taskId, string userId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);
            entity.AssignedUserId = userId;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }

        public async Task<VMTask> UnAssign(string taskId)
        {
            var entity = await DBContext.TaskItems.FindAsync(taskId);

            entity.AssignedUserId = null;

            DBContext.Update(entity);
            await DBContext.SaveChangesAsync();

            return await ReadAsync(entity.Id);
        }
    }

}
