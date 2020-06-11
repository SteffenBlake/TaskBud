using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskBud.Business.Data;
using TaskBud.Business.Models.Invitations;

namespace TaskBud.Business.Services
{
    public class InvitationManager
    {
        public ILogger<InvitationManager> Log { get; }
        private TaskBudConfig Config { get; }
        private TaskBudDbContext DbContext { get; }
        private UserManager<IdentityUser> UserManager { get; }

        public InvitationManager(
            ILogger<InvitationManager> log,
            TaskBudConfig config, 
            TaskBudDbContext dbContext, 
            UserManager<IdentityUser> userManager)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<VMInvitation> CreateAsync()
        {
            DateTimeOffset? expiration = null;

            if (Config.Invitations?.Expiry.HasValue ?? false)
            {
                expiration = DateTimeOffset.Now + Config.Invitations.Expiry.Value;
            }

            var invitation = new InvitationCode
            {
                Expiration = expiration
            };

            await DbContext.AddAsync(invitation);
            await DbContext.SaveChangesAsync();

            return await ReadAsync(invitation.Id);
        }


        public Task<VMInvitationIndex> IndexAsync()
        {
            var invitations = DbContext.InvitationCodes
                .Select(VMInvitation.Read(DbContext))
                .ToList();

            var data = new VMInvitationIndex
            {
                Invitations = invitations
            };

            return Task.FromResult(data);
        }

        public Task<VMInvitation> ReadAsync(string code)
        {
            var data = DbContext.InvitationCodes
                .Where(m => m.Id == code)
                .Select(VMInvitation.Read(DbContext))
                .ToList()
                .SingleOrDefault();

            return Task.FromResult(data);
        }

        public async Task<bool> ValidateAsync(string code)
        {
            var invitation = await DbContext.InvitationCodes.FindAsync(code);

            if (invitation == null)
            {
                return false;
            }

            if (invitation.UserId != null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> IsExpiredAsync(string code)
        {
            var invitation = await DbContext.InvitationCodes.FindAsync(code);

            if (invitation == null)
            {
                throw new InvalidOperationException($"Attempted to redeem a non-existing code '{code}'");
            }

            if (invitation.Expiration != null) 
                return invitation.Expiration >= DateTimeOffset.Now;

            // Expiry is disabled, short circuit out
            if (!Config.Invitations.Expiry.HasValue)
                return true;

            Log.LogWarning($"Rejected invalid invitation code without expiry date set.");
            return false;

        }

        public async Task<IdentityResult> TryConsumeAsync(string code, string userName)
        {
            var userTask = UserManager.FindByNameAsync(userName);
            var codeTask = DbContext.InvitationCodes.FindAsync(code);

            var invitation = (await codeTask);

            if (invitation?.User != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Invitation code '{code}' is invalid" });
            }

            invitation.User = (await userTask) ?? throw new InvalidOperationException($"User with UserName '{userName}' does not exist."); ;

            DbContext.Update(invitation);
            await DbContext.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<VMInvitation> ExpireAsync(string code)
        {
            var model = await DbContext.InvitationCodes.FindAsync(code);
            model.Expiration = DateTimeOffset.Now.AddMilliseconds(-1);
            DbContext.Update(model);
            await DbContext.SaveChangesAsync();

            return await ReadAsync(code);
        }
    }
}
