using Switcharoo.Model;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<FeatureStateResponse> GetFeatureStateAsync(string featureName, Guid key);

    Task<ToggleFeatureResponse> ToggleFeatureAsync(string featureName, Guid key, Guid authKey);
    
    Task<AddFeatureResponse> AddFeatureAsync(string featureName, string description, Guid key, Guid authKey);
    
    Task<DeleteFeatureResponse> DeleteFeatureAsync(string featureName, Guid key, Guid authKey);
}