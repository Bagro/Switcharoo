using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Teams.DeleteTeam;

public sealed class DeleteTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/team", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteTeam")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public async Task<IResult> HandleAsync(Guid teamId, ClaimsPrincipal user, ITeamRepository teamRepository)
    {
        var team = await teamRepository.GetTeamAsync(teamId, user.GetUserId());

        if (team is null)
        {
            return Results.BadRequest("Team not found");
        }
        
        if(team.Owner.Id != user.GetUserId())
        {
            return Results.BadRequest("You don't have permission to delete this team");
        }
        
        await teamRepository.DeleteTeamAsync(team);
        
        return Results.Ok();
    }
}
