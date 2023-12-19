using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Features.DeleteFeature;

public sealed class DeleteFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/feature", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteFeature")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status403Forbidden);
    }

    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.DeleteFeatureAsync(id, user.GetUserId());
        
        return result.wasDeleted ? Results.Ok() : Results.Forbid();
    }
}
