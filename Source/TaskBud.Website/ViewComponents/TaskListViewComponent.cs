using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Website.ViewComponents
{
    public class TaskGroupListViewComponent : ViewComponent
    {
        private ITaskGroupManager Manager { get; }

        public TaskGroupListViewComponent(ITaskGroupManager manager)
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
