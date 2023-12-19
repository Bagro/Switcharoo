using Switcharoo.Database.Entities;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Features;

public interface IFeatureRepository
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId);

    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task AddFeatureAsync(Feature feature);

    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task<(bool wasDeleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId);

    Task<(bool wasDeleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId);

    Task<List<Feature>> GetFeaturesAsync(Guid userId);

    Task<Feature?> GetFeatureAsync(Guid id, Guid userId);

    Task UpdateFeatureAsync(Feature feature);

    Task<bool> IsNameAvailableAsync(string name, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid featureId, Guid userId);

    Task<bool> IsKeyAvailableAsync(string key, Guid userId);

    Task<bool> IsKeyAvailableAsync(string key, Guid featureId, Guid userId);
    
    Task<Environment?> GetEnvironmentAsync(Guid environmentId, Guid getUserId);
}
