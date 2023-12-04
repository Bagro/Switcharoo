using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo;

public sealed class EnvironmentProvider(IRepository repository) : IEnvironmentProvider
{
    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId)
        {
            return await repository.AddEnvironmentAsync(environmentName, userId);
        }
    
        public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId)
        {
            var result = await repository.GetEnvironmentsAsync(userId);
    
            var environments = result.environments.Select(x => new Environment { Id = x.Id, Name = x.Name }).ToList();
    
            return (result.wasFound, environments, result.reason);
        }
        
        public async Task<(bool wasFound, Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId)
        {
            var result = await repository.GetEnvironmentAsync(id, userId);

            if (!result.wasFound || result.environment == null)
            {
                return (result.wasFound, null, result.reason);
            }

            var environment = new Environment{ Id = result.environment.Id, Name = result.environment.Name };
        
            return (result.wasFound, environment, result.reason);
        }

        public async Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Environment environment, Guid userId)
        {
            var result = await repository.UpdateEnvironmentAsync(environment, userId);
        
            return (result.wasUpdated, result.reason);
        }

        public async Task<(bool deleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId)
        {
            return await repository.DeleteEnvironmentAsync(id, userId);
        }
}
