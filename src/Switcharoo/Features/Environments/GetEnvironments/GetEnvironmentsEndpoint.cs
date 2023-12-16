using System.Security.Claims;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

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
        var result = await environmentRepository.GetEnvironmentsAsync(user.GetUserId());

        return result.wasFound ? Results.Ok(result.environments) : Results.NotFound(result.reason);
    }
}
