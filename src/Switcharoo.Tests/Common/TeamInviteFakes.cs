using Bogus;
using Switcharoo.Database.Entities;

namespace Switcharoo.Tests.Common;

public static class TeamInviteFakes
{
    public static TeamInvite GetFakeTeamInvite(int seed = 0)
    {
        var faker = new Faker<TeamInvite>()
            .RuleFor(x => x.InviteCode, f => f.Random.Guid())
            .RuleFor(x => x.ExpiresAt, f => f.Date.Future())
            .RuleFor(x => x.Team, (_, _) => TeamFakes.GetFakeTeam(seed))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past())
            .RuleFor(x => x.InvitedBy, (_, _) => UserFakes.GetFakeUser(seed));
        
        if (seed > 0)
        {
            faker.UseSeed(seed);
        }
        
        return faker.Generate();
    }
}
