using System;
using System.Linq.Expressions;
using TaskBud.Business.Data;

namespace TaskBud.Business.Models.Invitations
{
    public class VMInvitation
    {
        public string Id { get; set; }

        public InvitationState State { get; set; }


        public static Expression<Func<InvitationCode, VMInvitation>> Read(TaskBudDbContext db)
        {
            return 
                (code) =>
                    new VMInvitation
                    {
                        Id = code.Id,
                        State = 
                            code.UserId != null ? InvitationState.Accepted :
                            code.Expiration == null ? InvitationState.Pending :
                            code.Expiration.Value < DateTimeOffset.Now ? InvitationState.Expired :
                            InvitationState.Pending
                    };
        }
                
    }
}
