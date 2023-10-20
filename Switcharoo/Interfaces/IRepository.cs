namespace Switcharoo.Interfaces;

public interface IRepository
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid key);
    
    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(string featureName, Guid key, Guid authKey);
    
    Task<(bool wasAdded, string reason)> AddFeatureAsync(string featureName, string description, Guid key, Guid authKey);
    
    Task<(bool deleted, string reason)> DeleteFeatureAsync(string featureName, Guid key, Guid authKey);
    
    Task<bool> IsAdminAsync(Guid key, Guid authKey);
}
