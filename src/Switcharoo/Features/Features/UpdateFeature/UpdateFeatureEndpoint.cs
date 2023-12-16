using System.Security.Claims;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model;

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
        var feature = new Feature
        {
            Id = updateFeatureRequest.Id,
            Name = updateFeatureRequest.Name,
            Key = updateFeatureRequest.Key,
            Description = updateFeatureRequest.Description,
            Environments = updateFeatureRequest.Environments
                .Select(environment => new FeatureEnvironment(environment.IsEnabled, string.Empty, environment.Id))
                .ToList(),
        };
        
        if (!await featureRepository.IsNameAvailableAsync(feature.Name, feature.Id, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, feature.Id, user.GetUserId()))
        {
            return Results.Conflict("Key is already in use");
        }
        
        var result = await featureRepository.UpdateFeatureAsync(feature, user.GetUserId());
        
        return result.wasUpdated ? Results.Ok() : Results.BadRequest(result.reason);
    }
}
