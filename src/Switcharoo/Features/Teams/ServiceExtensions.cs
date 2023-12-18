using Switcharoo.Interfaces;

namespace Switcharoo.Features.Teams;

public static class ServiceExtensions
{
    public static void AddTeams(this IServiceCollection services)
    {
        services.AddScoped<ITeamRepository, TeamRepository>();
    }
}
