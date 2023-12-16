using Switcharoo.Interfaces;

namespace Switcharoo.Features.Environments;

public static class ServiceExtensions
{
    public static void AddEnvironments(this IServiceCollection services)
    {
        services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
    }
}
