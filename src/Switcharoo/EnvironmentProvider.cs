using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo;

public sealed class EnvironmentProvider(IRepository repository) : IEnvironmentProvider
{
    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId)
        {
            return await repository.AddEnvironmentAsync(environmentName, userId);
        }
    
        public Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId)
        {
            return repository.GetEnvironmentsAsync(userId);
        }
        
        public Task<(bool wasFound, Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId)
        {
            return repository.GetEnvironmentAsync(id, userId);
        }

        public Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Environment environment, Guid userId)
        {
            return repository.UpdateEnvironmentAsync(environment, userId);
        }

        public async Task<(bool deleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId)
        {
            return await repository.DeleteEnvironmentAsync(id, userId);
        }
}
