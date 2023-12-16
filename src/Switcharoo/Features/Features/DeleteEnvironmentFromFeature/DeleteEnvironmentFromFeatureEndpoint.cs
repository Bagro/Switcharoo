using System.Security.Claims;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Features.DeleteEnvironmentFromFeature;

public sealed class DeleteEnvironmentFromFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/feature/{featureId:guid}/environment/{environmentId:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteEnvironmentFromFeature")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public static async Task<IResult> HandleAsync(Guid featureId, Guid environmentId, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.DeleteEnvironmentFromFeatureAsync(featureId, environmentId, user.GetUserId());
        
        return result.wasDeleted ? Results.Ok() : Results.BadRequest(result.reason);
    }
}
