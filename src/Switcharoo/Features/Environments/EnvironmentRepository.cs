using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Features.Environments;

public sealed class EnvironmentRepository(BaseDbContext context) : IEnvironmentRepository
{
    public async Task AddEnvironmentAsync(Environment environment)
    {
        await context.Environments.AddAsync(environment);
        
        await context.SaveChangesAsync();
    }

    public async Task<List<Environment>> GetEnvironmentsAsync(Guid userId)
    {
        var environments = await context.Environments.Where(x => x.Owner.Id == userId).ToListAsync();

        return environments;
    }
    
    
    public async Task<Environment?> GetEnvironmentAsync(Guid id, Guid userId)
    {
        var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == id && x.Owner.Id == userId);

        return environment ?? null;
    }

    public async Task UpdateEnvironmentAsync(Environment environment)
    {
        context.Environments.Update(environment);

        await context.SaveChangesAsync();
    }

    public async Task<(bool wasDeleted, string reason)> DeleteEnvironmentAsync(Guid id, Guid userId)
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
        return !await context.Environments.AnyAsync(x => x.Owner.Id == userId && x.Name == name);
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid environmentId, Guid userId)
    {
        return !await context.Environments.AnyAsync(x => x.Owner.Id == userId && x.Id != environmentId && x.Name == name);
    }
}
