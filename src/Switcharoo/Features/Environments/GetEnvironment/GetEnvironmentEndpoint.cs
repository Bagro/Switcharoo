using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Environment = Switcharoo.Features.Environments.Model.Environment;

namespace Switcharoo.Features.Environments.GetEnvironment;

public sealed class GetEnvironmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/environment/{id:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetEnvironment")
            .WithOpenApi()
            .Produces<Environment>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        var result = await environmentRepository.GetEnvironmentAsync(id, user.GetUserId());
        
        return result.wasFound ? Results.Ok(result.environment) : Results.NotFound(result.reason);
    }
}
