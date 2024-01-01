using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Teams.CreateInviteCode;

public sealed class CreateInviteCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/team/invitecode", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("CreateInviteCode")
            .WithOpenApi()
            .Produces<CreateInviteCodeResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(CreateInviteCodeRequest request, ClaimsPrincipal user, ITeamRepository teamRepository, CancellationToken cancellationToken)
    {
        var storedUser = await teamRepository.GetUserAsync(user.GetUserId());

        if (storedUser is null)
        {
            return Results.BadRequest("User not found");
        }
        
        var team = await teamRepository.GetTeamAsync(request.TeamId);

        if (team is null)
        {
            return Results.BadRequest("Team not found");
        }

        if (team.Owner.Id != storedUser.Id)
        {
            return Results.Forbid();
        }

        var teamInvite = new TeamInvite()
        {
            InviteCode = Guid.NewGuid(),
            Team = team,
            InvitedBy = storedUser,
            CreatedAt = DateTimeOffset.Now,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ValidForDays),
        };

        await teamRepository.AddInviteCodeAsync(teamInvite);

        return Results.Ok(new CreateInviteCodeResponse(teamInvite.InviteCode, teamInvite.ExpiresAt, team.Name, team.Id));
    }
}