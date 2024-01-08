using Bogus;
using Switcharoo.Database.Entities;

namespace Switcharoo.Tests.Common;

public static class FeatureFakes
{
    public static Feature GetFakeFeature(int environmentCount = 1, int seed = 0)
    {
        var faker = new Faker<Feature>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Random.Word())
            .RuleFor(x => x.Description, f => f.Random.Word())
            .RuleFor(x => x.Key, f => f.Random.Word())
            .RuleFor(x => x.ShareWithTeam, f => f.Random.Bool())
            .RuleFor(x => x.Owner, (_, _) => UserFakes.GetFakeUser(seed))
            .RuleFor(x => x.Environments, (_, _) => FeatureEnvironmentFakes.GetFakeFeatureEnvironments(environmentCount, seed).ToList());
            
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
}
