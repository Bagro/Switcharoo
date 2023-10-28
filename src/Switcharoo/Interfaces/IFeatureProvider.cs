using Switcharoo.Entities;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentId);

    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
    
    Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid userId);
    
    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
    
    Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId);
    
    Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
    
    Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId);
    
    Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId);
    
    Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId);
}