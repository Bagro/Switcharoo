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
            .RuleFor(x => x.Owner, (_, _) => UserFakes.GetFakeUser(seed));
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
}
