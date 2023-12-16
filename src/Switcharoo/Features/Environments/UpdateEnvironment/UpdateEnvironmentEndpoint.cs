using System.Security.Claims;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Features.Environments.UpdateEnvironment;

public sealed class UpdateEnvironmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/environment", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("UpdateEnvironment")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(Environment environment, ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        if (!await environmentRepository.IsNameAvailableAsync(environment.Name, environment.Id, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }
        
        var result = await environmentRepository.UpdateEnvironmentAsync(environment, user.GetUserId());
        
        return result.wasUpdated ? Results.Ok() : Results.BadRequest(result.reason);
    }
}
