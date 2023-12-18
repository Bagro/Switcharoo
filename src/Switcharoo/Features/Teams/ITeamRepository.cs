using Switcharoo.Database.Entities;
using Switcharoo.Features.Teams.DeleteTeam;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Interfaces;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    
    Task UpdateTeamAsync(Team team);
    
    Task DeleteTeamAsync(Team team);
    
    Task<Team?> GetTeamAsync(Guid teamId, Guid userId);
    
    Task<(bool wasFound, List<Team> teams, string reason)> GetTeamsAsync(Guid userId);
    
    Task<List<Environment>> GetEnvironmentsForIdAsync(List<Guid> environmentIds, Guid userId);
    
    Task<bool> IsNameAvailableAsync(string name, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid teamId, Guid userId);
    
    Task<List<Feature>> GetFeaturesForIdAsync(List<Guid> featuresToAdd, Guid userId);
}
