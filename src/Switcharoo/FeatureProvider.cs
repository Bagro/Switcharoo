using Switcharoo.Interfaces;
using Switcharoo.Model;
using Environment = Switcharoo.Model.Environment;

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
        var result = await repository.GetEnvironmentsAsync(userId);

        var environments = result.environments.Select(x => new Environment { Id = x.Id, Name = x.Name }).ToList();

        return (result.wasFound, environments, result.reason);
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid userId)
    {
        var result = await repository.GetFeaturesAsync(userId);

        // Map the entities to the model including the environments
        var features = result.features.Select(
            x => new Feature
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Environments = x.Environments.Select(
                    y => new FeatureEnvironment { IsEnabled = y.IsEnabled, EnvironmentId = y.Environment.Id, EnvironmentName = y.Environment.Name }).ToList(),
            }).ToList();

        return (result.wasFound, features, result.reason);
    }

    public async Task<(bool wasFound, Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId)
    {
        var result = await repository.GetFeatureAsync(id, userId);

        if (!result.wasFound || result.feature == null)
        {
            return (result.wasFound, null, result.reason);
        }
        
        var feature = new Feature
        {
            Id = result.feature.Id,
            Name = result.feature.Name,
            Description = result.feature.Description,
            Environments = result.feature.Environments.Select(
                y => new FeatureEnvironment { IsEnabled = y.IsEnabled, EnvironmentId = y.Environment.Id, EnvironmentName = y.Environment.Name }).ToList(),
        };
            
        return (result.wasFound, feature, result.reason);
    }

    public Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Feature feature, Guid userId)
    {
        return repository.UpdateFeatureAsync(feature, userId);
    }
}
