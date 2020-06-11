using System;
using Microsoft.AspNetCore.Identity;

namespace TaskBud.Business
{
    public class TaskBudConfig
    {
        public ConnectionType? ConnectionType { get; set; } = Business.ConnectionType.INVALID;
        public string ConnectionString { get; set; }
        public TaskBudIdentityConfig Identity { get; set; }
        public InvitationsConfig Invitations { get; set; }
    }

    public class TaskBudIdentityConfig
    {
        public PasswordOptions Password { get; set; }
    }

    public class InvitationsConfig
    {
        public TimeSpan? Expiry { get; set; }
    }

    public enum ConnectionType
    {
        MSSQL,
        POSTGRES,
        INVALID
    }
}
