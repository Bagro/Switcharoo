using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Teams;

public sealed class TeamRepository(BaseDbContext context) : ITeamRepository
{
    public async Task AddTeamAsync(Team team)
    {
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
    }

    public async Task UpdateTeamAsync(Team team)
    {
        context.Teams.Update(team);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTeamAsync(Team team)
    { 
        context.Teams.Remove(team);
        await context.SaveChangesAsync();
    }

    public Task<Team?> GetTeamAsync(Guid teamId)
    {
        return context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .SingleOrDefaultAsync(x => x.Id == teamId);
    }

    public async Task<(bool wasFound, List<Team> teams, string reason)> GetTeamsAsync(Guid userId)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        
        if (user is null)
        {
            return (false, [], "User not found");
        }
        
        var teams = await context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .Where(x => x.Owner.Id == userId || !x.InviteOnly || x.Members.Contains(user))
            .ToListAsync();
        
        return teams.Count == 0 ? (false, [], "No teams found") : (true, teams, "");
    }

    public Task<List<Environment>> GetEnvironmentsForIdAsync(List<Guid> environmentIds, Guid userId)
    {
        return context.Environments
            .Include(x => x.Owner)
            .Where(x => environmentIds.Contains(x.Id) && x.Owner.Id == userId)
            .ToListAsync();
    }
    
    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await context.Teams.AnyAsync(x => x.Name == name);
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid teamId)
    {
        return !await context.Teams.AnyAsync(x => x.Id != teamId && x.Name == name);
    }

    public Task<List<Feature>> GetFeaturesForIdAsync(List<Guid> featuresToAdd, Guid userId)
    {
        return context.Features
            .Include(x => x.Owner)
            .Where(x => featuresToAdd.Contains(x.Id) && x.Owner.Id == userId)
            .ToListAsync();
    }

    public async Task AddInviteCodeAsync(TeamInvite teamInvite)
    {
        context.TeamInvites.Add(teamInvite);
        await context.SaveChangesAsync();
    }
    
    public Task<User?> GetUserAsync(Guid userId)
    {
        return context.Users.SingleOrDefaultAsync(x => x.Id == userId);
    }
}
