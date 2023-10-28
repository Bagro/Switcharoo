using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentId)
    {
        return await repository.GetFeatureStateAsync(featureName, environmentId);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        return await repository.ToggleFeatureAsync(featureId, environmentId, userId);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid userId)
    {
        return await repository.AddFeatureAsync(featureName, description, userId);
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

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId)
    {
        return await repository.AddEnvironmentAsync(environmentName, userId);
    }

    public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId)
    {
        return await repository.GetEnvironmentsAsync(userId);
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId)
    {
        return await repository.GetFeaturesAsync(userId);
    }
}
