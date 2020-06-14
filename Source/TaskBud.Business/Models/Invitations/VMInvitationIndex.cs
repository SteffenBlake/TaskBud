using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBud.Business.Models.Invitations
{
    public class VMInvitationIndex
    {
        public IList<VMInvitation> Invitations { get; set; }

        public bool ShowHidden { get; set; } = false;
    }
}
