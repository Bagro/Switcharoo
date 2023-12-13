using Switcharoo.Model.Requests;

namespace Switcharoo.Interfaces;

public interface ITeamProvider
{
    Task<(bool wasAdded, Guid id, string reason)> AddTeamAsync(AddTeamRequest request, Guid getUserId);
    Task<(bool wasUpdated, string reason)> UpdateTeamAsync(UpdateTeamRequest request, Guid getUserId);
}
