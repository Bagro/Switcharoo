using Switcharoo.Entities;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey);

    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey);
    
    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey);
    
    Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey);
    
    Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey);
    
    Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid authKey);
    
    Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid authKey);
    
    Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid authKey);
}