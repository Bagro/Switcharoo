using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Providers;

public sealed class FeatureProvider(IFeatureRepository featureRepository) : IFeatureProvider
{
    public Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId)
    {
        return featureRepository.GetFeatureStateAsync(featureKey, environmentId);
    }

    public Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return featureRepository.ToggleFeatureAsync(featureId, environmentId, userId);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(Feature feature, Guid userId)
    {
        if (!await featureRepository.IsNameAvailableAsync(feature.Name, userId))
        {
            return (false, Guid.Empty, "Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, userId))
        {
            return (false, Guid.Empty, "Key is already in use");
        }

        return await featureRepository.AddFeatureAsync(feature, userId);
    }

    public Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return featureRepository.AddEnvironmentToFeatureAsync(featureId, environmentId, userId);
    }

    public Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId)
    {
        return featureRepository.DeleteFeatureAsync(featureId, userId);
    }

    public  Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return featureRepository.DeleteEnvironmentFromFeatureAsync(featureId, environmentId, userId);
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId)
    {
        var result = await featureRepository.GetFeaturesAsync(userId);

        return (result.wasFound, result.features, result.reason);
    }

    public  Task<(bool wasFound, Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId)
    {
        return featureRepository.GetFeatureAsync(id, userId);
    }

    public async Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Feature feature, Guid userId)
    {
        if (!await featureRepository.IsNameAvailableAsync(feature.Name, feature.Id, userId))
        {
            return (false, "Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, feature.Id, userId))
        {
            return (false, "Key is already in use");
        }

        return await featureRepository.UpdateFeatureAsync(feature, userId);
    }
}
