using Switcharoo.Model;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<FeatureStateResponse> GetFeatureStateAsync(string featureName, Guid environmentKey);

    Task<ToggleFeatureResponse> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<AddFeatureResponse> AddFeatureAsync(string featureName, string description, Guid authKey);
    
    Task<AddEnvironmentToFeatureResponse> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<DeleteFeatureResponse> DeleteFeatureAsync(Guid featureKey, Guid authKey);
    
    Task<DeleteFeatureResponse> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
}