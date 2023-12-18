using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Features.AddEnvironmentToFeature;

public sealed class AddEnvironmentToFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/feature/environment", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AddEnvironmentToFeature")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public static async Task<IResult> HandleAsync(AddEnvironmentToFeatureRequest request, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.AddEnvironmentToFeatureAsync(request.FeatureId, request.EnvironmentId, user.GetUserId());
        
        return result.wasAdded ? Results.Ok() : Results.BadRequest(result.reason);
    }
}
