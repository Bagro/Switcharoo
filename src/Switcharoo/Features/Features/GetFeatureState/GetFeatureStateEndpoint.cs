using Switcharoo.Common;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Features.GetFeatureState;

public sealed class GetFeatureStateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/feature/{featureKey}/environment/{environmentId:guid}", handler: HandleAsync)
            .WithName("GetFeatureState")
            .WithOpenApi()
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(string featureKey, Guid environmentId, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.GetFeatureStateAsync(featureKey, environmentId);
        
        return  result.wasFound ? Results.Ok(result.isActive) : Results.NotFound();        
    }
}
