using Switcharoo.Model;
using Switcharoo.Model.Requests;

namespace Switcharoo.Interfaces;

public interface ITeamRepository
{
    Task<(bool wasAdded, Guid id, string reason)> AddTeamAsync(AddTeamRequest request, Guid userId);
    
    Task<(bool wasUpdated, string reason)> UpdateTeamAsync(UpdateTeamRequest request, Guid userId);
    
    Task<(bool wasDeleted, string reason)> DeleteTeamAsync(DeleteTeamRequest request, Guid userId);
    
    Task<(bool wasFound, Team? team, string reason)> GetTeamAsync(Guid teamId, Guid userId);
    
    Task<(bool wasFound, List<Team> teams, string reason)> GetTeamsAsync(Guid userId);
    
    Task<bool> IsNameAvailableAsync(string name, Guid userId);

    Task<bool> IsNameAvailableAsync(string name, Guid teamId, Guid userId);
}
