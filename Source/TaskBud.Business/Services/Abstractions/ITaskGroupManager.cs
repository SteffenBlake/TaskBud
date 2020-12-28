using System.Collections.Generic;
using System.Threading.Tasks;
using TaskBud.Business.Models.TaskGroups;

namespace TaskBud.Business.Services.Abstractions
{
    /// <summary>
    /// Injectable Service for managing Task Groups
    /// </summary>
    public interface ITaskGroupManager
    {
        /// <summary>
        /// Creates a new Task Group
        /// </summary>
        /// <param name="data">Data to Create it with</param>
        /// <returns>The Id of the newly created Task</returns>
        Task<string> CreateAsync(VMTaskGroup data);

        /// <summary>
        /// List of all the Task Groups
        /// </summary>
        Task<IList<VMTaskGroup>> IndexAsync();

        /// <summary>
        /// Updates a Task Group
        /// </summary>
        /// <param name="data">The Data to update the task group with</param>
        Task UpdateAsync(VMTaskGroup data);
    }
}