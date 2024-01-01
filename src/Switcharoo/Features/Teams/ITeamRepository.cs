using Switcharoo.Database.Entities;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Interfaces;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    
    Task UpdateTeamAsync(Team team);
    
    Task DeleteTeamAsync(Team team);
    
    Task<Team?> GetTeamAsync(Guid teamId);
    
    Task<(bool wasFound, List<Team> teams, string reason)> GetTeamsAsync(Guid userId);
    
    Task<List<Environment>> GetEnvironmentsForIdAsync(List<Guid> environmentIds, Guid userId);
    
    Task<bool> IsNameAvailableAsync(string name);

    Task<bool> IsNameAvailableAsync(string name, Guid teamId);
    
    Task<List<Feature>> GetFeaturesForIdAsync(List<Guid> featuresToAdd, Guid userId);
    
    Task AddInviteCodeAsync(TeamInvite teamInvite);
}
