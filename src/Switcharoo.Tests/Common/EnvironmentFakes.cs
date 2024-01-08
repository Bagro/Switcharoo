using Bogus;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Tests.Common;

public static class EnvironmentFakes
{
    public static Environment GetFakeEnvironment(int seed = 0)
    {
        var faker = GetFaker(seed);
        
        return faker.Generate();
    }
    
    public static List<Environment> GetFakeEnvironments(int numberOfEnvironments = 2, int seed = 0)
    {
        var faker = GetFaker(seed);

        return faker.Generate(numberOfEnvironments);
    }

    private static Faker<Environment> GetFaker(int seed)
    {
        var faker = new Faker<Environment>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Random.Word())
            .RuleFor(x => x.ShareWithTeam, f => f.Random.Bool());

        if (seed > 0)
        {
            faker.UseSeed(seed);
        }

        return faker;
    }
}
