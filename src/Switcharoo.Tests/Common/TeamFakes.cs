using Bogus;
using Switcharoo.Database.Entities;

namespace Switcharoo.Tests.Common;

public static class TeamFakes
{
    public static Team GetFakeTeam(int seed = 0)
    {
        var faker = new Faker<Team>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Random.Word())
            .RuleFor(x => x.Description, f => f.Random.Word())
            .RuleFor(x => x.Owner, (_, _) => UserFakes.GetFakeUser(seed))
            .RuleFor(x => x.Environments, (_, t) => GetTeamEnvironments(t))
            .RuleFor(x => x.Features, (_, t) => TeamFeatures(t))
            .RuleFor(x => x.Members, (_, _) => new List<User>());
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }

    private static List<TeamFeature> TeamFeatures(Team team)
    {
        return [
            new TeamFeature
            {
                Id = Guid.NewGuid(),
                Feature = FeatureFakes.GetFakeFeature(),
                IsReadOnly = false,
                AllCanToggle = false,
                Team = team,
            },
            new TeamFeature
            {
                Id = Guid.NewGuid(),
                Feature = FeatureFakes.GetFakeFeature(),
                IsReadOnly = false,
                AllCanToggle = false,
                Team = team,
            }];
    }

    private static List<TeamEnvironment> GetTeamEnvironments(Team team)
    {
        return [
            new TeamEnvironment
            {
                Id = Guid.NewGuid(),
                Environment = EnvironmentFakes.GetFakeEnvironment(),
                IsReadOnly = false,
                Team = team,
            },
            new TeamEnvironment
            {
                Id = Guid.NewGuid(),
                Environment = EnvironmentFakes.GetFakeEnvironment(),
                IsReadOnly = false,
                Team = team,
            }];
    }
}
