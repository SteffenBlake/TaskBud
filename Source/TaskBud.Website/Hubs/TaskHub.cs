using Microsoft.AspNetCore.SignalR;

namespace TaskBud.Website.Hubs
{
    public class TaskHub : Hub
    {
        public static string EndPoint => "/taskHub";

        public static string Assigned => "Assigned";

        public static string UnAssigned => "Un-Assigned";

        public static string Completed => "Completed";

        public static string ReOpened => "Re-Opened";

        public static string Updated => "Updated";

        public static string Created => "Created";
    }
}
