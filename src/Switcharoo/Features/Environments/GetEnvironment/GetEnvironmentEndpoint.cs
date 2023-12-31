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
            .Produces<string>(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        var environment = await environmentRepository.GetEnvironmentAsync(id, user.GetUserId());
        
        if (environment is null)
        {
            return Results.NotFound("Environment not found");
        }
        
        var returnEnvironment = new Environment
        {
            Id = environment.Id,
            Name = environment.Name,
        };
        
        return Results.Ok(returnEnvironment);
    }
}
