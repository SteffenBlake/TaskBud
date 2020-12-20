using System;
using System.Linq;
using System.Threading.Tasks;
using TaskBud.Business.Data;

namespace TaskBud.Business.Services
{
    /// <summary>
    /// Injectable service for management of Api Access Tokens
    /// </summary>
    public class ApiTokenManager
    {
        private TaskBudDbContext DBContext { get; }

        public ApiTokenManager(TaskBudDbContext dbContext)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets the users associated Api Token, if they have one
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>The users token, or null if they don't have one.</returns>
        public string ForUser(string userId)
        {
            return DBContext.ApiTokens.FirstOrDefault(t => t.UserId == userId)?.Token ?? null;
        }

        /// <summary>
        /// Generates a new Api Access Token for a given user, deleting any old tokens they had prior
        /// </summary>
        /// <param name="userId">Id of the User to generate a token for</param>
        /// <returns>The generated Api Access Token</returns>
        public async Task<string> GenerateAsync(string userId)
        {
            ClearTokens(userId);

            var entity = new ApiToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString()
            };

            await DBContext.AddAsync(entity);
            await DBContext.SaveChangesAsync();

            return entity.Token;
        }

        /// <summary>
        /// Validates an API Access Token and returns the user Id associated with it
        /// </summary>
        /// <param name="token">The authorization token for the user</param>
        /// <returns>Associated User Id for <see cref="token"/></returns>
        public async Task<string> ValidateAsync(string token)
        {
            return (await DBContext.FindAsync<ApiToken>(token)).UserId;
        }

        /// <summary>
        /// Deletes all existing Api Access Tokens for a given user
        /// </summary>
        /// <param name="userId">Id of the user to clear tokens for</param>
        public async Task ClearAsync(string userId)
        {
            ClearTokens(userId);
            await DBContext.SaveChangesAsync();
        }

        private void ClearTokens(string userId)
        {
            var oldTokens = DBContext.ApiTokens.Where(t => t.UserId == userId);
            DBContext.RemoveRange(oldTokens);
        }
    }
}
