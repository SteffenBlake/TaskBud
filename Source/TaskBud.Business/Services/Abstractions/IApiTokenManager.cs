using System.Threading.Tasks;

namespace TaskBud.Business.Services.Abstractions
{
    /// <summary>
    /// Injectable service for management of Api Access Tokens
    /// </summary>
    public interface IApiTokenManager
    {
        /// <summary>
        /// Gets the users associated Api Token, if they have one
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>The users token, or null if they don't have one.</returns>
        string ForUser(string userId);

        /// <summary>
        /// Generates a new Api Access Token for a given user, deleting any old tokens they had prior
        /// </summary>
        /// <param name="userId">Id of the User to generate a token for</param>
        /// <returns>The generated Api Access Token</returns>
        Task<string> GenerateAsync(string userId);

        /// <summary>
        /// Validates an API Access Token and returns the user Id associated with it
        /// </summary>
        /// <param name="token">The authorization token for the user</param>
        /// <returns>Associated User Id for <see cref="token"/></returns>
        Task<string> ValidateAsync(string token);

        /// <summary>
        /// Deletes all existing Api Access Tokens for a given user
        /// </summary>
        /// <param name="userId">Id of the user to clear tokens for</param>
        Task ClearAsync(string userId);
    }
}