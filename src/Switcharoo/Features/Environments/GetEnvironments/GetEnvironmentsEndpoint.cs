using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Environment = Switcharoo.Features.Environments.Model.Environment;

namespace Switcharoo.Features.Environments.GetEnvironments;

public sealed class GetEnvironmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/environments", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetEnvironments")
            .WithOpenApi()
            .Produces<List<Environment>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(ClaimsPrincipal user, IEnvironmentRepository environmentRepository, CancellationToken cancellationToken)
    {
        var environments = await environmentRepository.GetEnvironmentsAsync(user.GetUserId());
        
        if (!environments.Any())
        {
            return Results.NotFound("No environments found");
        }
        
        var returnEnvironments = environments.Select(
            environment => new Environment
            {
                Id = environment.Id,
                Name = environment.Name,
            })
            .ToList();

        return Results.Ok(returnEnvironments);
    }
}
