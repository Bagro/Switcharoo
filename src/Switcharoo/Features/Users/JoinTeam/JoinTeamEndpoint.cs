using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Users.Shared;

namespace Switcharoo.Features.Users.JoinTeam;

public sealed class JoinTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/user/jointeam/{teamId:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("JoinTeam")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public async Task<IResult> HandleAsync(Guid teamId, ClaimsPrincipal user, IUserFeatureRepository userFeatureRepository, CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        var storedUser = await userFeatureRepository.GetUserAsync(userId);
        
        if (storedUser is null)
        {
            return Results.BadRequest("User does not exist");
        }
        
        if (storedUser.Team is not null)
        {
            return Results.Conflict("User is already a member of a team");
        }
        
        var team = await userFeatureRepository.GetTeamAsync(teamId);
        
        if (team is null)
        {
            return Results.BadRequest("Team does not exist");
        }

        if (team.InviteOnly)
        {
            return Results.BadRequest("Team is invite only");
        }

        if (team.Members.Exists(x => x.Id == userId))
        {
            return Results.Conflict("You are already a member of this team");
        }
        
        team.Members.Add(storedUser);
        
        await UserSharedToTeamHelper.AddUsersFeaturesToTeam(team, storedUser, userFeatureRepository);
        await UserSharedToTeamHelper.AddUsersEnvironmentsToTeam(team, storedUser, userFeatureRepository);

        await userFeatureRepository.UpdateTeamAsync(team);

        return Results.Ok();
    }
}
