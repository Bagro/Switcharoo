using Switcharoo.Database.Entities;

namespace Switcharoo.Features.Users.Shared;

public static class UserSharedToTeamHelper
{
    public static async Task AddUsersEnvironmentsToTeam(Team team, User storedUser, IUserFeatureRepository userRepository)
    {
        var usersEnvironments = await userRepository.GetSharedEnvironments(storedUser.Id);

        if (usersEnvironments.Count == 0)
        {
            return;
        }

        foreach (var environment in usersEnvironments)
        {
            team.Environments.Add(
                new TeamEnvironment
                {
                    Environment = environment,
                    Team = team,
                    Id = Guid.NewGuid(),
                    IsReadOnly = storedUser.DefaultTeamReadOnly,
                });
        }
    }

    public static async Task AddUsersFeaturesToTeam(Team team, User storedUser, IUserFeatureRepository userRepository)
    {
        var usersFeatures = await userRepository.GetSharedFeatures(storedUser.Id);

        if (usersFeatures.Count == 0)
        {
            return;
        }

        foreach (var feature in usersFeatures)
        {
            team.Features.Add(
                new TeamFeature
                {
                    Feature = feature,
                    Team = team,
                    Id = Guid.NewGuid(),
                    AllCanToggle = storedUser.DefaultTeamAllowToggle,
                    IsReadOnly = storedUser.DefaultTeamReadOnly,
                });
        }
    }
}
