using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Interfaces;

public interface IEnvironmentProvider
{
    Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId);
    
    Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId);
    
    Task<(bool wasFound, Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId);
    
    Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Environment environment, Guid userId);
    
    Task<(bool deleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId);
}
