using Switcharoo.Common;
using Switcharoo.Database.Entities;

namespace Switcharoo.Features.Users;

public interface IUserFeatureRepository : IUserRepository
{
    Task<TeamInvite?> GetTeamInviteAsync(Guid inviteCode);
    
    Task<Team?> GetTeamAsync(Guid teamId);
    
    Task UpdateTeamAsync(Team team);
}
