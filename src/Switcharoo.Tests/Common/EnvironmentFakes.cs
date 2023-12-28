using Bogus;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Tests.Common;

public static class EnvironmentFakes
{
    public static Environment GetFakeEnvironment(int seed = 0)
    {
        var faker = new Faker<Environment>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Random.Word());
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
}
