using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Users.AcceptTeamInvite;

public sealed class AcceptTeamInviteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/user/acceptinvite/{code:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AcceptTeamInvite")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public async Task<IResult> HandleAsync(Guid code, ClaimsPrincipal user, IUserFeatureRepository userRepository, CancellationToken cancellationToken)
    {
        var storedUser = await userRepository.GetUserAsync(user.GetUserId());

        if (storedUser is null)
        {
            return Results.BadRequest("User not found");
        }

        if (storedUser.Team is not null)
        {
            return Results.Conflict("User is already a member of a team");
        }
        
        var teamInvite = await userRepository.GetTeamInviteAsync(code);
        
        if (teamInvite is null)
        {
            return Results.BadRequest("Team invite not found");
        }

        if (teamInvite.ExpiresAt < DateTime.UtcNow)
        {
            return Results.BadRequest("Team invite has expired");
        }

        if (teamInvite.ActivatedByUser is not null)
        {
            return Results.BadRequest("Team invite has already been used");
        }
        
        var team = await userRepository.GetTeamAsync(teamInvite.Team.Id);

        if (team is null)
        {
            return Results.BadRequest("Team not found");
        }

        if (team.Members.Exists(x => x.Id == storedUser.Id))
        {
            return Results.BadRequest("User is already a member of this team");
        }

        team.Members.Add(storedUser);

        await userRepository.UpdateTeamAsync(team);

        return Results.Ok();
    }
}
