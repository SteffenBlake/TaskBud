using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Data;
using TaskBud.Business.Models.TaskGroups;
using TaskBud.Business.Services;

namespace TaskBud.Website.ViewComponents
{
    public class TaskGroupListViewComponent : ViewComponent
    {
        private TaskGroupManager Manager { get; }

        public TaskGroupListViewComponent(TaskGroupManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await Manager.IndexAsync();
            return View(data);
        }
    }
}
