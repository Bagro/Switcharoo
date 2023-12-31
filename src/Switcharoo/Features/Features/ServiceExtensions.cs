namespace Switcharoo.Features.Features;

public static class ServiceExtensions
{
    public static void AddFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureRepository, FeatureRepository>();
    }
}
