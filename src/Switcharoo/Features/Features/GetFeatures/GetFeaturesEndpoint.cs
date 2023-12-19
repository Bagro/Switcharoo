using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.Model;

namespace Switcharoo.Features.Features.GetFeatures;

public sealed class GetFeaturesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/features", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetFeatures")
            .WithOpenApi()
            .Produces<List<Feature>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(ClaimsPrincipal user, IFeatureRepository featureRepository)
    {
        var features = await featureRepository.GetFeaturesAsync(user.GetUserId());
        
        var returnFeatures = features.Select(
            x => new Feature
            {
                Id = x.Id,
                Name = x.Name,
                Key = x.Key,
                Description = x.Description,
                Environments = x.Environments.OrderBy(e => e.Environment.Name).Select(y => new Model.FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList(),
            }).ToList();
        
       return returnFeatures.Count == 0
            ? Results.NotFound("No features found")
            : Results.Ok(returnFeatures);
    }
}
