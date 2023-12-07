using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo;

public sealed class FeatureRepository(BaseDbContext context) : IRepository
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

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(Model.Feature feature, Guid userId)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var featureToAdd = new Feature { Id = Guid.NewGuid(), Name = feature.Name, Key = feature.Key, Description = feature.Description, Owner = user, Environments = new List<FeatureEnvironment>() };

        foreach (var environment in feature.Environments)
        {
            var existingEnvironment = await context.Environments.FirstOrDefaultAsync(x => x.Id == environment.EnvironmentId && x.Owner.Id == userId);
            if (existingEnvironment == null)
            {
                continue;
            }

            featureToAdd.Environments.Add(
                new FeatureEnvironment
                {
                    Feature = featureToAdd,
                    Environment = existingEnvironment,
                    IsEnabled = environment.IsEnabled,
                    Id = Guid.NewGuid(),
                });
        }

        await context.Features.AddAsync(featureToAdd);
        await context.SaveChangesAsync();

        return (true, feature.Id, "Feature added");
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

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureId, Guid userId)
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

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureId, Guid environmentId, Guid userId)
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

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid userId)
    {
        if (await context.Environments.AnyAsync(x => x.Name == environmentName && x.Owner.Id == userId))
        {
            return (false, Guid.Empty, "Environment already exists");
        }

        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var environment = new Environment { Id = Guid.NewGuid(), Name = environmentName, Owner = user, Features = new List<FeatureEnvironment>() };

        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();

        return (true, environment.Id, "Environment added");
    }

    public async Task<(bool wasFound, List<Model.Environment> environments, string reason)> GetEnvironmentsAsync(Guid userId)
    {
        var environments = await context.Environments.Where(x => x.Owner.Id == userId).ToListAsync();

        return (environments.Count != 0, environments.Select(
            x => new Model.Environment
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList(), environments.Count != 0 ? "Environments found" : "No environments found");
    }

    public async Task<(bool wasFound, List<Model.Feature> features, string reason)> GetFeaturesAsync(Guid userId)
    {
        var features = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).Where(x => x.Owner.Id == userId).ToListAsync();

        return (features.Count != 0, features.Select(
            x => new Model.Feature
            {
                Id = x.Id,
                Name = x.Name,
                Key = x.Key,
                Description = x.Description,
                Environments = x.Environments.OrderBy(e => e.Environment.Name).Select(y => new Model.FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList(),
            }).ToList(), features.Count != 0 ? "Features found" : "No features found");
    }

    public async Task<(bool wasFound, Model.Feature? feature, string reason)> GetFeatureAsync(Guid id, Guid userId)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == id && x.Owner.Id == userId);

        if (feature == null)
        {
            return (false, null, "Feature not found");
        }

        feature.Environments = feature.Environments.OrderBy(x => x.Environment.Name).ToList();

        return (true, new Model.Feature
        {
            Id = feature.Id,
            Name = feature.Name,
            Key = feature.Key,
            Description = feature.Description,
            Environments = feature.Environments.Select(
                y => new Model.FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList(),
        }, "Feature found");
    }

    public async Task<(bool wasUpdated, string reason)> UpdateFeatureAsync(Model.Feature feature, Guid userId)
    {
        var existingFeature = await context.Features.Include(x => x.Environments).ThenInclude(x => x.Environment).SingleOrDefaultAsync(x => x.Id == feature.Id && x.Owner.Id == userId);

        if (existingFeature == null)
        {
            return (false, "Feature not found");
        }

        existingFeature.Name = feature.Name;
        existingFeature.Key = feature.Key;
        existingFeature.Description = feature.Description;
        var existingEnvironments = existingFeature.Environments.Select(x => x.Environment.Id).ToList();
        var newEnvironments = feature.Environments.Select(x => x.EnvironmentId).ToList();

        var environmentsToAdd = newEnvironments.Except(existingEnvironments).ToList();
        var environmentsToRemove = existingEnvironments.Except(newEnvironments).ToList();

        foreach (var environmentId in environmentsToAdd)
        {
            var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId);
            if (environment == null)
            {
                return (false, "Environment not found");
            }

            existingFeature.Environments.Add(
                new FeatureEnvironment
                {
                    Feature = existingFeature,
                    Environment = environment,
                    IsEnabled = feature.Environments.Single(x => x.EnvironmentId == environmentId).IsEnabled,
                });
        }

        foreach (var environmentId in environmentsToRemove)
        {
            var featureEnvironment = existingFeature.Environments.SingleOrDefault(x => x.Environment.Id == environmentId);
            if (featureEnvironment == null)
            {
                continue;
            }

            existingFeature.Environments.Remove(featureEnvironment);
        }

        await context.SaveChangesAsync();

        return (true, "Feature updated");
    }

    public async Task<(bool wasFound, Model.Environment? environment, string reason)> GetEnvironmentAsync(Guid id, Guid userId)
    {
        var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == id && x.Owner.Id == userId);

        if (environment == null)
        {
            return (false, null, "Environment not found");
        }

        return (true, new Model.Environment
        {
            Id = environment.Id,
            Name = environment.Name,
        }, "Environment found");
    }

    public async Task<(bool wasUpdated, string reason)> UpdateEnvironmentAsync(Model.Environment environment, Guid userId)
    {
        var existingEnvironment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environment.Id && x.Owner.Id == userId);

        if (existingEnvironment == null)
        {
            return (false, "Environment not found");
        }

        existingEnvironment.Name = environment.Name;

        await context.SaveChangesAsync();

        return (true, "Environment updated");
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId)
    {
        var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == id && x.Owner.Id == userId);

        if (environment == null)
        {
            return (false, "Environment not found");
        }

        context.Environments.Remove(environment);

        await context.SaveChangesAsync();

        return (true, "Environment deleted");
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
}
