using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams.Model;
using Switcharoo.Features.Teams.Shared;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Teams.GetTeams;

public sealed class GetTeamsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/teams", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetTeams")
            .WithOpenApi()
            .Produces<List<Team>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(ClaimsPrincipal user, ITeamRepository teamRepository, CancellationToken cancellationToken)
    {
        var result = await teamRepository.GetTeamsAsync(user.GetUserId());
        
        if (!result.wasFound)
        {
            return Results.NotFound(result.reason);
        }

        var teams = result.teams.Select(team => TeamFactory.CreateFromEntity(team, user.GetUserId())).ToList();

        return Results.Ok(teams);
    }
}
