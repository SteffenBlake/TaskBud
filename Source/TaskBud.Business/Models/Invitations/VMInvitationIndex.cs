using System.Collections.Generic;

namespace TaskBud.Business.Models.Invitations
{
    public class VMInvitationIndex
    {
        public IList<VMInvitation> Invitations { get; set; }

        public bool ShowHidden { get; set; } = false;
    }
}
