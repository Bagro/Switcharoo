using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId)
    {
        return await repository.GetFeatureStateAsync(featureKey, environmentId);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return await repository.ToggleFeatureAsync(featureId, environmentId, userId);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(Feature feature, Guid userId)
    {
        if (!await repository.IsNameAvailableAsync(feature.Name, userId))
        {
            return (false, Guid.Empty, "Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await repository.IsKeyAvailableAsync(feature.Key, userId))
        {
            return (false, Guid.Empty, "Key is already in use");
        }

        return await repository.AddFeatureAsync(feature, userId);
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return await repository.AddEnvironmentToFeatureAsync(featureId, environmentId, userId);
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId)
    {
        return await repository.DeleteFeatureAsync(featureId, userId);
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return await repository.DeleteEnvironmentFromFeatureAsync(featureId, environmentId, userId);
    }


    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId)
    {
        var result = await repository.GetFeaturesAsync(userId);

        return (result.wasFound, result.features, result.reason);
    }

    public async Task<(bool wasFound, Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId)
    {
        var result = await repository.GetFeatureAsync(id, userId);

        if (!result.wasFound || result.feature == null)
        {
            return (result.wasFound, null, result.reason);
        }

        return (result.wasFound, result.feature, result.reason);
    }

    public async Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Feature feature, Guid userId)
    {
        if (!await repository.IsNameAvailableAsync(feature.Name, feature.Id, userId))
        {
            return (false, "Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await repository.IsKeyAvailableAsync(feature.Key, feature.Id, userId))
        {
            return (false, "Key is already in use");
        }

        return await repository.UpdateFeatureAsync(feature, userId);
    }
}
