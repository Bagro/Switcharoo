using Switcharoo.Model;

namespace Switcharoo.Interfaces;

public interface IRepository
{
    Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey);

    Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);

    Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey);

    Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey);

    Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey);

    Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey);

    Task<bool> IsAdminAsync(Guid authKey);

    Task<bool> IsFeatureAdminAsync(Guid featureKey, Guid authKey);

    Task<bool> IsEnvironmentAdminAsync(Guid environmentKey, Guid authKey);
}
