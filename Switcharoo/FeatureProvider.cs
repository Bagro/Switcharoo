using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<FeatureStateResponse> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        var featureStateAsync = await repository.GetFeatureStateAsync(featureName, environmentKey);

        return new FeatureStateResponse(featureName, featureStateAsync.isActive);
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
        
        var toggleFeatureAsync = await repository.ToggleFeatureAsync(featureKey, environmentKey, authKey);
        
        return new ToggleFeatureResponse(featureKey.ToString(), toggleFeatureAsync.isActive, toggleFeatureAsync.wasChanged, toggleFeatureAsync.reason);
    }

    public async Task<AddFeatureResponse> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return new AddFeatureResponse(featureName, false, "Not authorized");
        }
        
        var addFeatureAsync = await repository.AddFeatureAsync(featureName, description, authKey);
        
        return new AddFeatureResponse(featureName, addFeatureAsync.wasAdded, addFeatureAsync.reason);
    }
    
    public async Task<AddFeatureResponse> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return new AddFeatureResponse(featureKey.ToString(), false, "Not authorized");
        }
        
        var addFeatureAsync = await repository.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
        
        return new AddFeatureResponse(featureKey.ToString(), addFeatureAsync.wasAdded, addFeatureAsync.reason);
    }

    public async Task<DeleteFeatureResponse> DeleteFeatureAsync(Guid featureKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin)
        {
            return new DeleteFeatureResponse(featureKey.ToString(), false, "Not authorized");
        }
        
        var deleteFeatureAsync = await repository.DeleteFeatureAsync(featureKey);
        
        return new DeleteFeatureResponse(featureKey.ToString(), deleteFeatureAsync.deleted, deleteFeatureAsync.reason);
    }
    
    public async Task<DeleteFeatureResponse> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return new DeleteFeatureResponse(featureKey.ToString(), false, "Not authorized");
        }
        
        var deleteFeatureAsync = await repository.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
        
        return new DeleteFeatureResponse(featureKey.ToString(), deleteFeatureAsync.deleted, deleteFeatureAsync.reason);
    }
}
