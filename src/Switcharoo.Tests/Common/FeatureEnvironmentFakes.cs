using Bogus;
using Switcharoo.Database.Entities;

namespace Switcharoo.Tests.Common;

public static class FeatureEnvironmentFakes
{
    public static FeatureEnvironment GetFakeFeatureEnvironment(int seed = 0)
    {
        var faker = GetFaker(seed);
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
    
    public static List<FeatureEnvironment> GetFakeFeatureEnvironments(int count, int seed = 0)
    {
        var faker = GetFaker(seed);
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate(count);
    }

    private static Faker<FeatureEnvironment> GetFaker(int seed)
    {
        var faker = new Faker<FeatureEnvironment>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Feature, (_, _) => FeatureFakes.GetFakeFeature(seed))
            .RuleFor(x => x.Environment, (_, _) => EnvironmentFakes.GetFakeEnvironment(seed))
            .RuleFor(x => x.IsEnabled, f => f.Random.Bool());
        return faker;
    }
}
