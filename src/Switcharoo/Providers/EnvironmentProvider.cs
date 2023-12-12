using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Providers;

public sealed class EnvironmentProvider(IEnvironmentRepository environmentRepository) : IEnvironmentProvider
{
    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId)
        {
            return await environmentRepository.AddEnvironmentAsync(environmentName, userId);
        }
    
        public Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId)
        {
            return environmentRepository.GetEnvironmentsAsync(userId);
        }
        
        public Task<(bool wasFound, Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId)
        {
            return environmentRepository.GetEnvironmentAsync(id, userId);
        }

        public Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Environment environment, Guid userId)
        {
            return environmentRepository.UpdateEnvironmentAsync(environment, userId);
        }

        public async Task<(bool deleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId)
        {
            return await environmentRepository.DeleteEnvironmentAsync(id, userId);
        }
}
