using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.Model;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Features.GetFeature;

public sealed class GetFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/feature/{id:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetFeature")
            .WithOpenApi()
            .Produces<Feature>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.GetFeatureAsync(id, user.GetUserId());

        return result.wasFound ? Results.Ok(result.feature) : Results.NotFound();
    }
}
