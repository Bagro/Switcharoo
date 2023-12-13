using Switcharoo.Interfaces;
using Switcharoo.Model;
using Switcharoo.Model.Requests;

namespace Switcharoo.Providers;

public sealed class TeamProvider(ITeamRepository teamRepository) : ITeamProvider
{
    public async Task<(bool wasAdded, Guid id, string reason)> AddTeamAsync(AddTeamRequest request, Guid userId)
    {
        var isNameAvailable = await teamRepository.IsNameAvailableAsync(request.Name, userId);
        
        if (!isNameAvailable)
        {
            return (false, Guid.Empty, "There is already a team with that name");
        }
        
        return await teamRepository.AddTeamAsync(request, userId);
    }

    public async Task<(bool wasUpdated, string reason)> UpdateTeamAsync(UpdateTeamRequest request, Guid userId)
    {
        var isNameAvailable = await teamRepository.IsNameAvailableAsync(request.Name, request.Id, userId);
        
        if (!isNameAvailable)
        {
            return (false, "There is already a team with that name");
        }
        
        return await teamRepository.UpdateTeamAsync(request, userId);
    }

    public Task<(bool wasDeleted, string reason)> DeleteTeamAsync(DeleteTeamRequest request, Guid userId)
    {
        return teamRepository.DeleteTeamAsync(request, userId);
    }

    public Task<(bool wasFound, Team? team, string reason)> GetTeamAsync(Guid teamId, Guid userId)
    {
        return teamRepository.GetTeamAsync(teamId, userId);
    }

    public Task<(bool wasFound, List<Team> teams, string reason)> GetTeamsAsync(Guid userId)
    {
        return teamRepository.GetTeamsAsync(userId);
    }
}
