using System;
using System.Linq;
using System.Threading.Tasks;
using TaskBud.Business.Data;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Business.Services.Implementations
{
    /// <summary>
    /// Non-injectable implementation for <see cref="IApiTokenManager"/>
    /// For Dependency injection, inject the <see cref="IApiTokenManager"/>
    /// </summary>
    public class ApiTokenManager : IApiTokenManager
        {
        private TaskBudDbContext DBContext { get; }

        public ApiTokenManager(TaskBudDbContext dbContext)
        {
            DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <inheritdoc/>
        public string ForUser(string userId)
        {
            return DBContext.ApiTokens.FirstOrDefault(t => t.UserId == userId)?.Token ?? null;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<string> ValidateAsync(string token)
        {
            return (await DBContext.FindAsync<ApiToken>(token)).UserId;
        }

        /// <inheritdoc/>
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
