using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        return await repository.GetFeatureStateAsync(featureName, environmentKey);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        return await repository.ToggleFeatureAsync(featureKey, environmentKey, authKey);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        return await repository.AddFeatureAsync(featureName, description, authKey);
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        return await repository.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey)
    {
       return await repository.DeleteFeatureAsync(featureKey);
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        return await repository.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid authKey)
    {
        return await repository.AddEnvironmentAsync(environmentName, authKey);
    }

    public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid authKey)
    {
        return await repository.GetEnvironmentsAsync(authKey);
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid authKey)
    {
        return await repository.GetFeaturesAsync(authKey);
    }
}
