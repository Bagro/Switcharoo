using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<FeatureStateResponse> GetFeatureStateAsync(string featureName, Guid key)
    {
        var featureStateAsync = await repository.GetFeatureStateAsync(featureName, key);

        return new FeatureStateResponse(featureName, featureStateAsync.isActive);
    }

    public async Task<ToggleFeatureResponse> ToggleFeatureAsync(string featureName, Guid key, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(key, authKey);
        if (!isAdmin)
        {
            return new ToggleFeatureResponse(featureName, false, false, "Not authorized");
        }
        
        var toggleFeatureAsync = await repository.ToggleFeatureAsync(featureName, key, authKey);
        
        return new ToggleFeatureResponse(featureName, toggleFeatureAsync.isActive, toggleFeatureAsync.wasChanged, toggleFeatureAsync.reason);
    }

    public async Task<AddFeatureResponse> AddFeatureAsync(string featureName, string description, Guid key, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(key, authKey);
        if (!isAdmin)
        {
            return new AddFeatureResponse(featureName, false, "Not authorized");
        }
        
        var addFeatureAsync = await repository.AddFeatureAsync(featureName, description, key, authKey);
        
        return new AddFeatureResponse(featureName, addFeatureAsync.wasAdded, addFeatureAsync.reason);
    }

    public async Task<DeleteFeatureResponse> DeleteFeatureAsync(string featureName, Guid key, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(key, authKey);
        if (!isAdmin)
        {
            return new DeleteFeatureResponse(featureName, false, "Not authorized");
        }
        
        var deleteFeatureAsync = await repository.DeleteFeatureAsync(featureName, key, authKey);
        
        return new DeleteFeatureResponse(featureName, deleteFeatureAsync.deleted, deleteFeatureAsync.reason);
    }
}
