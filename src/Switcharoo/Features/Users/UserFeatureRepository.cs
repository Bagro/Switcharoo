using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;

namespace Switcharoo.Features.Users;

public sealed class UserFeatureRepository(BaseDbContext context) : IUserFeatureRepository
{
    public Task<User?> GetUserAsync(Guid userId)
    {
        return context.Users.SingleOrDefaultAsync(x => x.Id == userId);
    }

    public Task<TeamInvite?> GetTeamInviteAsync(Guid inviteCode)
    {
        return context.TeamInvites.SingleOrDefaultAsync(x => x.InviteCode == inviteCode);
    }

    public Task<Team?> GetTeamAsync(Guid teamId)
    {
        return context.Teams.Include(x => x.Members).SingleOrDefaultAsync(x => x.Id == teamId);
    }

    public async Task UpdateTeamAsync(Team team)
    {
        context.Teams.Update(team);
        await context.SaveChangesAsync();
    }
}
