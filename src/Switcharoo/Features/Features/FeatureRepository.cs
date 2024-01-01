using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Features;

public sealed class FeatureRepository(BaseDbContext context) : IFeatureRepository
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureKey, Guid environmentId)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Key == featureKey && x.Environments.Any(y => y.Environment.Id == environmentId));

        var active = feature?.Environments.SingleOrDefault(x => x.Environment.Id == environmentId)?.IsEnabled;

        return (active ?? false, active != null);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == featureId && x.Environments.Any(y => y.Environment.Id == environmentId) && x.Owner.Id == userId);

        if (feature == null)
        {
            return (false, false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.Single(x => x.Environment.Id == environmentId);

        featureEnvironment.IsEnabled = !featureEnvironment.IsEnabled;

        await context.SaveChangesAsync();

        return (featureEnvironment.IsEnabled, true, "Feature toggled");
    }

    public async Task AddFeatureAsync(Feature feature)
    {
        await context.Features.AddAsync(feature);
        
        await context.SaveChangesAsync();
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        var featureExist = context.Features.SingleOrDefaultAsync(x => x.Id == featureId && x.Owner.Id == userId);
        var environmentExist = context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId && x.Owner.Id == userId);
        var featureEnvironmentExist = context.FeatureEnvironments.AnyAsync(x => x.Feature.Id == featureId && x.Environment.Id == environmentId);


        Task.WaitAll(featureExist, environmentExist, featureEnvironmentExist);
        if (featureExist.Result == null || environmentExist.Result == null)
        {
            return (false, "Feature or environment not found");
        }

        if (featureEnvironmentExist.Result)
        {
            return (false, "Feature environment already exists");
        }

        var featureEnvironment = new FeatureEnvironment { Id = Guid.NewGuid(), Feature = featureExist.Result, Environment = environmentExist.Result, IsEnabled = false };

        await context.FeatureEnvironments.AddAsync(featureEnvironment);

        await context.SaveChangesAsync();

        return (true, "Feature environment added");
    }

    public async Task<(bool wasDeleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId)
    {
        var feature = await context.Features.Include(x => x.Environments).SingleOrDefaultAsync(x => x.Id == featureId && x.Owner.Id == userId);

        if (feature == null)
        {
            return (false, "Feature not found");
        }

        context.Features.Remove(feature);

        await context.SaveChangesAsync();

        return (true, "Feature deleted");
    }

    public async Task<(bool wasDeleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == featureId && x.Owner.Id == userId);

        if (feature == null)
        {
            return (false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentId);

        if (featureEnvironment == null)
        {
            return (false, "Feature environment not found");
        }

        feature.Environments.Remove(featureEnvironment);

        await context.SaveChangesAsync();

        return (true, "Feature environment deleted");
    }
  

    public Task<List<Feature>> GetFeaturesAsync(Guid userId)
    {
        return context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).Where(x => x.Owner.Id == userId).ToListAsync();
    }

    public Task<Feature?> GetFeatureAsync(Guid id, Guid userId)
    {
        return  context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == id && x.Owner.Id == userId);
    }

    public async Task UpdateFeatureAsync(Feature feature)
    {
        context.Features.Update(feature);

        await context.SaveChangesAsync();
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid userId)
    {
        return !await context.Features.AnyAsync(x => x.Owner.Id == userId && x.Name == name);
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid featureId, Guid userId)
    {
        return !await context.Features.AnyAsync(x => x.Id != featureId && x.Owner.Id == userId && x.Name == name);
    }

    public async Task<bool> IsKeyAvailableAsync(string key, Guid userId)
    {
        return !await context.Features.AnyAsync(x => x.Owner.Id == userId && x.Key == key);
    }

    public async Task<bool> IsKeyAvailableAsync(string key, Guid featureId, Guid userId)
    {
        return !await context.Features.AnyAsync(x => x.Id != featureId && x.Owner.Id == userId && x.Key == key);
    }

    public Task<Environment?> GetEnvironmentAsync(Guid environmentId, Guid getUserId)
    {
        return context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId && x.Owner.Id == getUserId);
    }
    
    public Task<User?> GetUserAsync(Guid userId)
    {
        return context.Users.SingleOrDefaultAsync(x => x.Id == userId);
    }
}
