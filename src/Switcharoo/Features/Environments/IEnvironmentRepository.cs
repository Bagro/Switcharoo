using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Features.Environments;

public interface IEnvironmentRepository
{
    Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId);
    
    Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId);
    
    Task<(bool wasFound, Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId);
    
    Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Model.Environment environment, Guid userId);
    
    Task<(bool wasDeleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid userId);
    
    Task<bool> IsNameAvailableAsync(string name, Guid environmentId, Guid userId);
}
