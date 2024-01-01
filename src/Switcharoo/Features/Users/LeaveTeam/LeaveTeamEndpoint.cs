using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Users.LeaveTeam;

public sealed class LeaveTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/user/leaveteam/{teamId:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("LeaveTeam")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
    }
    
    public async Task<IResult> HandleAsync(Guid teamId, ClaimsPrincipal user, IUserFeatureRepository userFeatureRepository, CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        var storedUser = await userFeatureRepository.GetUserAsync(userId);
        
        if (storedUser is null)
        {
            return Results.BadRequest("User does not exist");
        }
        
        if (storedUser.Team is null)
        {
            return Results.BadRequest("User is not a member of a team");
        }
        
        var team = await userFeatureRepository.GetTeamAsync(teamId);
        
        if (team is null)
        {
            return Results.BadRequest("Team does not exist");
        }

        if (!team.Members.Exists(x => x.Id == userId))
        {
            return Results.BadRequest("User is not a member of this team");
        }
        
        team.Members.Remove(storedUser);

        await userFeatureRepository.UpdateTeamAsync(team);

        return Results.Ok();
    }
}
