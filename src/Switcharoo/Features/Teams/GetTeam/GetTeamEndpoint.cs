using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams.Shared;

namespace Switcharoo.Features.Teams.GetTeam;

public sealed class GetTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/team/{id:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetTeam")
            .WithOpenApi()
            .Produces<Model.Team>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, ITeamRepository teamRepository)
    {
        var team = await teamRepository.GetTeamAsync(id);
        
        if (team is null)
        {
            return Results.NotFound("Team not found");
        }
        
        if (!UserAllowedToLoadTeam(user.GetUserId(), team))
        {
            return Results.Forbid();
        }

        var returnTeam = TeamFactory.CreateFromEntity(team, user.GetUserId());

        return Results.Ok(returnTeam);
    }
    
    private static bool UserAllowedToLoadTeam(Guid userId, Team team)
    {
        return team.Owner.Id == userId || (team.AllCanManage && team.Members.Exists(x => x.Id == userId));
    }
}
