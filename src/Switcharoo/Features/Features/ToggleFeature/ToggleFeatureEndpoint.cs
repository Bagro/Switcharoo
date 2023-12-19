using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Features.ToggleFeature;

public sealed class ToggleFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/feature/toggle", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("ToggleFeature")
            .WithOpenApi()
            .Produces<ToggleFeatureResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public static async Task<IResult> HandleAsync(ToggleFeatureRequest request, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var result = await featureRepository.ToggleFeatureAsync(request.FeatureId, request.EnvironmentId, user.GetUserId());
        
        return result.wasChanged ? Results.Ok(new ToggleFeatureResponse(request.FeatureId.ToString(), result.isActive, result.wasChanged, result.reason)) : Results.BadRequest(result.reason);
    }
}
