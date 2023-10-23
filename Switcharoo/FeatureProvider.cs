using Microsoft.AspNetCore.Authentication;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo;

public sealed class FeatureProvider(IRepository repository) : IFeatureProvider
{
    public async Task<IResult> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        var featureState = await repository.GetFeatureStateAsync(featureName, environmentKey);

        return Results.Ok(new FeatureStateResponse(featureName, featureState.isActive));
    }

    public async Task<IResult> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.ToggleFeatureAsync(featureKey, environmentKey, authKey);
        
        return Results.Ok(new ToggleFeatureResponse(featureKey.ToString(), result.isActive, result.wasChanged, result.reason));
    }

    public async Task<IResult> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.AddFeatureAsync(featureName, description, authKey);
        
        return result.wasAdded ? Results.Ok(new AddResponse(featureName, result.key, result.wasAdded, result.reason)) : Results.BadRequest(result.reason);
    }
    
    public async Task<IResult> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
        
        return result.wasAdded ? Results.Ok() : Results.BadRequest(result.reason);
    }

    public async Task<IResult> DeleteFeatureAsync(Guid featureKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.DeleteFeatureAsync(featureKey);
        
        return result.deleted ? Results.Ok() : Results.BadRequest(result.reason);
    }
    
    public async Task<IResult> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        var isFeatureAdmin = await repository.IsFeatureAdminAsync(featureKey, authKey);
        var isEnvironmentAdmin = await repository.IsEnvironmentAdminAsync(environmentKey, authKey);
        
        if (!isAdmin || !isFeatureAdmin || !isEnvironmentAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
        
        return result.deleted ? Results.Ok() : Results.BadRequest(result.reason);
    }

    public async Task<IResult> AddEnvironmentAsync(string environmentName, Guid authKey)
    {
        var isAdmin = await repository.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return Results.Forbid(new AuthenticationProperties(), new[] { "Not authorized" });
        }
        
        var result = await repository.AddEnvironmentAsync(environmentName, authKey);
        
        return result.wasAdded ? Results.Ok(new AddResponse(environmentName, result.key, result.wasAdded, result.reason)) : Results.BadRequest(result.reason);
    }
}
