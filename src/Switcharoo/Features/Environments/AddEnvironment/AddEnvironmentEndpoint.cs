using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Environments.AddEnvironment;

public sealed class AddEnvironmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/environment", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AddEnvironment")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(AddEnvironmentRequest request, ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        
        if (!await environmentRepository.IsNameAvailableAsync(request.Name, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }

        var storedUser = await environmentRepository.GetUserAsync(user.GetUserId());
        
        if (storedUser is null)
        {
            return Results.BadRequest("User not found");
        }
        
        var environment = new Environment { Id = Guid.NewGuid(), Name = request.Name, Owner = storedUser, Features = [] };
        
        await environmentRepository.AddEnvironmentAsync(environment);
        
        return Results.Ok();
    }
}
