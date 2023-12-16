using Switcharoo.Model;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Interfaces;

public interface IFeatureRepository
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId);

    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(Model.Feature feature, Guid userId);

    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId);

    Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId);

    Task<(bool wasFound, Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId);

    Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Model.Feature feature, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid featureId, Guid userId);

    Task<bool> IsKeyAvailableAsync(string key, Guid userId);

    Task<bool> IsKeyAvailableAsync(string key, Guid featureId, Guid userId);
}
