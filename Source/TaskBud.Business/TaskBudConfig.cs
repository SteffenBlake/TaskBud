using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskBud.Business
{
    public class TaskBudConfig
    {
        public ConnectionType? ConnectionType { get; set; } = Business.ConnectionType.INVALID;
        public string ConnectionString { get; set; }
        public TaskBudIdentityConfig Identity { get; set; }
        public InvitationsConfig Invitations { get; set; }
        public LoggingConfig Logging { get; set; }
    }

    public class TaskBudIdentityConfig
    {
        public PasswordOptions Password { get; set; }
    }

    public class InvitationsConfig
    {
        public TimeSpan? Expiry { get; set; }
    }

    public class LoggingConfig
    {
        public string Path { get; set; }
        public LogLevel MinimumLevel { get; set; }
    }

    public enum ConnectionType
    {
        MSSQL,
        POSTGRES,
        INVALID
    }
}
