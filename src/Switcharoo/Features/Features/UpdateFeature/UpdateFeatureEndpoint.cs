using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Features.UpdateFeature;

public sealed class UpdateFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/feature", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("UpdateFeature")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(UpdateFeatureRequest updateFeatureRequest, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        if (!await featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }

        var feature = await featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId());

        if (feature is null)
        {
            return Results.NotFound("Feature not found");
        }
        
        feature.Name = updateFeatureRequest.Name;
        feature.Description = updateFeatureRequest.Description;
        feature.Key = updateFeatureRequest.Key;
        
        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, feature.Id, user.GetUserId()))
        {
            return Results.Conflict("Key is already in use");
        }

        await UpdateEnvironments(feature, updateFeatureRequest, user.GetUserId(), featureRepository);
        
        await featureRepository.UpdateFeatureAsync(feature);
        
        return Results.Ok();
    }

    private static async Task UpdateEnvironments(Feature feature, UpdateFeatureRequest updateFeatureRequest, Guid userId, IFeatureRepository featureRepository)
    {
        var existingEnvironments = feature.Environments.Select(x => x.Environment.Id).ToList();
        var newEnvironments = updateFeatureRequest.Environments.Select(x => x.Id).ToList();

        var environmentsToAdd = newEnvironments.Except(existingEnvironments).ToList();
        var environmentsToRemove = existingEnvironments.Except(newEnvironments).ToList();

        foreach (var environmentId in environmentsToAdd)
        {
            var environment = await featureRepository.GetEnvironmentAsync(environmentId, userId);
            if (environment == null)
            {
                continue;
            }

            feature.Environments.Add(
                new FeatureEnvironment
                {
                    Feature = feature,
                    Environment = environment,
                    IsEnabled = updateFeatureRequest.Environments.Single(x => x.Id == environmentId).IsEnabled,
                });
        }

        foreach (var environmentId in environmentsToRemove)
        {
            var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentId);
            if (featureEnvironment == null)
            {
                continue;
            }

            feature.Environments.Remove(featureEnvironment);
        }
    }
}
