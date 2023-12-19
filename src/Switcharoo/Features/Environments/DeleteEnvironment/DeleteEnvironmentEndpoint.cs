using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Environments.DeleteEnvironment;

public sealed class DeleteEnvironmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/environment", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteEnvironment")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public static async Task<IResult> HandleAsync(Guid environmentId, ClaimsPrincipal user, IEnvironmentRepository environmentProvider, CancellationToken cancellationToken)
    {
        var result = await environmentProvider.DeleteEnvironmentAsync(environmentId, user.GetUserId());
        
        return result.wasDeleted ? Results.Ok() : Results.BadRequest(result.reason);
    }
}
