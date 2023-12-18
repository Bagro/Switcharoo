using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.Model;
using Switcharoo.Interfaces;

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
        var result = await featureRepository.GetFeaturesAsync(user.GetUserId());
        
        return result.wasFound ? Results.Ok(result.features) : Results.NotFound(result.reason); 
    }
}
