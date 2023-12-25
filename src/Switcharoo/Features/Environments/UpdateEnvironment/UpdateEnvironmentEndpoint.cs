using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Environment = Switcharoo.Features.Environments.Model.Environment;

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
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(Environment environment, ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        if (!await environmentRepository.IsNameAvailableAsync(environment.Name, environment.Id, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }
        
        var storedEnvironment = await environmentRepository.GetEnvironmentAsync(environment.Id, user.GetUserId());
        
        if (storedEnvironment is null)
        {
            return Results.NotFound("Environment not found");
        }
        
        storedEnvironment.Name = environment.Name;
        
        await environmentRepository.UpdateEnvironmentAsync(storedEnvironment);
        
        return Results.Ok();
    }
}
