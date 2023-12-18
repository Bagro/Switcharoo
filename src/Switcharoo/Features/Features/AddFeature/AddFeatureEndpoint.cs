using System.Security.Claims;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Features.Features.AddFeature;

public sealed class AddFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/feature", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AddFeature")
            .WithOpenApi()
            .Produces<AddFeatureResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(AddFeatureRequest request, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var feature = new Feature
        {
            Name = request.Name,
            Key = request.Key,
            Description = request.Description,
            Environments = request.Environments?
                .Select(environment => new FeatureEnvironment(environment.IsEnabled, string.Empty, environment.Id))
                .ToList() ?? [],
        };

        if (!await featureRepository.IsNameAvailableAsync(feature.Name, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, user.GetUserId()))
        {
            return Results.Conflict("Key is already in use");
        }

        var result = await featureRepository.AddFeatureAsync(feature, user.GetUserId());
     
        return result.wasAdded ? Results.Ok(new AddFeatureResponse(request.Name, result.key)) : Results.BadRequest(result.reason);
    }
}
