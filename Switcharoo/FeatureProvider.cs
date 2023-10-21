using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<FeatureStateResponse> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        var featureState = await repository.GetFeatureStateAsync(featureName, environmentKey);

        return new FeatureStateResponse(featureName, featureState.isActive);
    }

    public async Task<ToggleFeatureResponse> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return new ToggleFeatureResponse(featureKey.ToString(), false, false, "Not authorized");
        }
        
        var result = await repository.ToggleFeatureAsync(featureKey, environmentKey, authKey);
        
        return new ToggleFeatureResponse(featureKey.ToString(), result.isActive, result.wasChanged, result.reason);
    }

    public async Task<AddFeatureResponse> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return new AddFeatureResponse(featureName, Guid.Empty, false, "Not authorized");
        }
        
        var result = await repository.AddFeatureAsync(featureName, description, authKey);
        
        return new AddFeatureResponse(featureName, result.key, result.wasAdded, result.reason);
    }
    
    public async Task<AddEnvironmentToFeatureResponse> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return new AddEnvironmentToFeatureResponse(false, "Not authorized");
        }
        
        var result = await repository.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
        
        return new AddEnvironmentToFeatureResponse(result.wasAdded, result.reason);
    }

    public async Task<DeleteFeatureResponse> DeleteFeatureAsync(Guid featureKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin)
        {
            return new DeleteFeatureResponse(false, "Not authorized");
        }
        
        var result = await repository.DeleteFeatureAsync(featureKey);
        
        return new DeleteFeatureResponse(result.deleted, result.reason);
    }
    
    public async Task<DeleteFeatureResponse> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return new DeleteFeatureResponse(false, "Not authorized");
        }
        
        var result = await repository.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
        
        return new DeleteFeatureResponse(result.deleted, result.reason);
    }
}
