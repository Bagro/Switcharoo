using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Interfaces;
using Switcharoo.Entities;
using Switcharoo.Model.Requests;

namespace Switcharoo.Repositories;

public sealed class TeamRepository(BaseDbContext context) : ITeamRepository
{
    public async Task<(bool wasAdded, Guid id, string reason)> AddTeamAsync(AddTeamRequest request, Guid userId)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            AllCanManage = request.AllCanManage,
            InviteOnly = request.InviteOnly,
            Owner = user,
        };
        
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        return (true, team.Id, "Team added");
    }

    public Task<(bool wasUpdated, string reason)> UpdateTeamAsync(UpdateTeamRequest request, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<(bool wasDeleted, string reason)> DeleteTeamAsync(DeleteTeamRequest request, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<(bool wasFound, Model.Team? team, string reason)> GetTeamAsync(Guid teamId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<(bool wasFound, List<Model.Team> teams, string reason)> GetTeamsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsNameAvailableAsync(string name, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsNameAvailableAsync(string name, Guid teamId, Guid userId)
    {
        throw new NotImplementedException();
    }
}
