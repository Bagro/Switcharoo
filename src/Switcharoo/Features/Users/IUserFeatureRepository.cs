using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Users;

public interface IUserFeatureRepository : IUserRepository
{
    Task<TeamInvite?> GetTeamInviteAsync(Guid inviteCode);
    
    Task<Team?> GetTeamAsync(Guid teamId);
    
    Task UpdateTeamAsync(Team team);
    
    Task<List<Feature>> GetSharedFeatures(Guid userId);
    
    Task<List<Environment>> GetSharedEnvironments(Guid userId);
}
