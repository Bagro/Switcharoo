using Switcharoo.Common;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Environments;

public interface IEnvironmentRepository : IUserRepository
{
    Task AddEnvironmentAsync(Environment environment);
    
    Task<List<Environment>> GetEnvironmentsAsync(Guid userId);
    
    Task<Environment?> GetEnvironmentAsync(Guid id, Guid userId);
    
    Task UpdateEnvironmentAsync(Environment environment);
    
    Task<(bool wasDeleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid userId);
    
    Task<bool> IsNameAvailableAsync(string name, Guid environmentId, Guid userId);
}
