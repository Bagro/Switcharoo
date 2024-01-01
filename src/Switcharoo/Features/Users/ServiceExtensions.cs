namespace Switcharoo.Features.Users;

public static class ServiceExtensions
{
    public static void AddUsers(this IServiceCollection services)
    {
        services.AddScoped<IUserFeatureRepository, UserFeatureRepository>();
    }
}
