using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskBud.Business.Models.Invitations;

namespace TaskBud.Business.Services.Abstractions
{
    /// <summary>
    /// Injectable service for management of Invitation Codes
    /// </summary>
    public interface IInvitationManager
    {
        /// <summary>
        /// Creates a new Invitation
        /// </summary>
        Task<VMInvitation> CreateAsync();

        /// <summary>
        /// Lists off all the invitations
        /// </summary>
        /// <param name="showHidden">Whether to display already consumed invitations</param>
        Task<VMInvitationIndex> IndexAsync(bool showHidden = false);

        /// <summary>
        /// Reads the invitation data for a specific invitation code
        /// </summary>
        /// <param name="code">The code of the invitation</param>
        Task<VMInvitation> ReadAsync(string code);

        /// <summary>
        /// Validates an invitation code as still usable
        /// </summary>
        /// <param name="code">The invitation code</param>
        /// <returns>Whether the code is valid or not</returns>
        Task<bool> ValidateAsync(string code);

        /// <summary>
        /// Checks if an invitation code is expired or not
        /// </summary>
        /// <param name="code">The invitation code to check</param>
        /// <returns>Whether it is expired or not</returns>
        Task<bool> IsExpiredAsync(string code);

        /// <summary>
        /// Attempts to consume an invitation code for a user
        /// </summary>
        /// <param name="code">The invitation code to try consuming</param>
        /// <param name="userName">Username of the user who is consuming the code</param>
        /// <returns>The success or fail result of the identity task</returns>
        Task<IdentityResult> TryConsumeAsync(string code, string userName);

        /// <summary>
        /// Expires an invitation code
        /// </summary>
        /// <param name="code">The code to expire</param>
        Task<VMInvitation> ExpireAsync(string code);
    }
}