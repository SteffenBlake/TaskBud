using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TaskBud.Business.Hubs
{
    public class TaskHub : Hub
    {
        public static string EndPoint => "/taskHub";

        public static string Assigned => "Assigned";
        public async Task Assign(string userId, string taskId)
        {
            await Clients.All.SendAsync(Assigned, userId, taskId);
        }

        public static string UnAssigned => "Un-Assigned";
        public async Task UnAssign(string previousUserId, string taskId)
        {
            await Clients.All.SendAsync(UnAssigned, previousUserId, taskId);
        }

        public static string Completed => "Completed";
        public async Task Complete(string taskId)
        {
            await Clients.All.SendAsync("Completed", taskId);
        }

        public static string ReOpened => "Re-Opened";
        public async Task ReOpen(string userId, string taskId)
        {
            await Clients.All.SendAsync(ReOpened, userId, taskId);
        }

        public static string Updated => "Updated";
        public async Task Update(string taskId)
        {
            await Clients.All.SendAsync(Updated, taskId);
        }

        public static string Created => "Created";
        public async Task Create(string taskId)
        {
            await Clients.All.SendAsync(Created, taskId);
        }
    }
}
