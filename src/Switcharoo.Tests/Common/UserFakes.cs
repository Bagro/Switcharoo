using Bogus;
using Switcharoo.Database.Entities;

namespace Switcharoo.Tests.Common;

public static class UserFakes
{
    public static User GetFakeUser(int seed = 0)
    {
        var faker = new Faker<User>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Email, f => f.Internet.Email());

        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
}
