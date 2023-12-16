using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Repositories;

public sealed class EnvironmentRepository(BaseDbContext context) : IEnvironmentRepository
{
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
}
