using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo;

public sealed class FeatureRepository(AppDbContext context) : IRepository
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Name == featureName && x.Environments.Any(y => y.Environment.Id == environmentKey));

        var active = feature?.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey)?.IsEnabled;

        return (active ?? false, active != null);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == featureKey && x.Environments.Any(y => y.Environment.Id == environmentKey));

        if (feature == null)
        {
            return (false, false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.Single(x => x.Environment.Id == environmentKey);

        featureEnvironment.IsEnabled = !featureEnvironment.IsEnabled;

        await context.SaveChangesAsync();

        return (featureEnvironment.IsEnabled, true, "Feature toggled");
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        if (await context.Features.AnyAsync(x => x.Name == featureName && x.Owner.Id == authKey))
        {
            return (false, Guid.Empty, "Feature already exists");
        }

        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == authKey);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var feature = new Feature { Id = Guid.NewGuid(), Name = featureName, Description = description, Owner = user, Environments = new List<FeatureEnvironment>() };

        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();

        return (true, feature.Id, "Feature added");
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == featureKey);
        var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environmentKey);

        if (feature == null || environment == null)
        {
            return (false, "Feature or environment not found");
        }

        if (feature.Environments.Any(x => x.Environment.Id == environmentKey))
        {
            return (false, "Feature environment already exists");
        }

        var featureEnvironment = new FeatureEnvironment { Id = Guid.NewGuid(), Feature = feature, Environment = environment, IsEnabled = false };

        feature.Environments.Add(featureEnvironment);

        await context.SaveChangesAsync();

        return (true, "Feature environment added");
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey)
    {
        var feature = await context.Features.Include(x => x.Environments).SingleOrDefaultAsync(x => x.Id == featureKey);

        if (feature == null)
        {
            return (false, "Feature not found");
        }

        context.Features.Remove(feature);

        await context.SaveChangesAsync();

        return (true, "Feature deleted");
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == featureKey);

        if (feature == null)
        {
            return (false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey);

        if (featureEnvironment == null)
        {
            return (false, "Feature environment not found");
        }

        feature.Environments.Remove(featureEnvironment);

        await context.SaveChangesAsync();

        return (true, "Feature environment deleted");
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid authKey)
    {
        if (await context.Environments.AnyAsync(x => x.Name == environmentName && x.Owner.Id == authKey))
        {
            return (false, Guid.Empty, "Environment already exists");
        }

        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == authKey);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var environment = new Environment { Id = Guid.NewGuid(), Name = environmentName, Owner = user, Features = new List<FeatureEnvironment>() };

        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();

        return (true, environment.Id, "Environment added");
    }

    public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid authKey)
    {
        var environments = await context.Environments.Where(x => x.Owner.Id == authKey).ToListAsync();

        return (environments.Count != 0, environments, environments.Count != 0 ? "Environments found" : "No environments found");
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid authKey)
    {
        var features = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).Where(x => x.Owner.Id == authKey).ToListAsync();

        return (features.Count != 0, features, features.Count != 0 ? "Features found" : "No features found");
    }
}
