using Microsoft.AspNetCore.Identity;
using System;

namespace TaskBud.Business.Data
{
    public class InvitationCode : EntityBase
    {
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public DateTimeOffset? Expiration { get; set; }
    }
}
