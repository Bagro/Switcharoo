using Switcharoo.Model;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(Feature feature, Guid userId);
    
    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
    
    Task<(bool wasDeleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId);

    Task<(bool wasDeleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
    
    Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Feature feature, Guid userId);

    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId);
    
    Task<(bool wasFound, Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId);
    
    Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId);
    
    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId);
}