using System.Security.Claims;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;

namespace Switcharoo.Features.Teams.AddTeam;

public sealed class AddTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/team", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AddTeam")
            .WithOpenApi()
            .Produces<AddTeamResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public async Task<IResult> HandleAsync(AddTeamRequest request, ClaimsPrincipal user, ITeamRepository teamRepository, IUserRepository userRepository, CancellationToken cancellationToken)
    {
        var storedUser = await userRepository.GetUserAsync(user.GetUserId());

        if (storedUser is null)
        {
            return Results.BadRequest("User not found");
        }
        
        var isNameAvailable = await teamRepository.IsNameAvailableAsync(request.Name, storedUser.Id);
        
        if (!isNameAvailable)
        {
            return Results.Conflict("Name is already in use");
        }
        
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            AllCanManage = request.AllCanManage,
            InviteOnly = request.InviteOnly,
            Owner = storedUser,
        };
        
        await teamRepository.AddTeamAsync(team);
        
        return Results.Ok(new AddTeamResponse(team.Name, team.Id));
    }
}
