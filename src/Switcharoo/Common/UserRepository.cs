using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;

namespace Switcharoo.Common;

public sealed class UserRepository(BaseDbContext context) : IUserRepository
{
    public Task<User?> GetUserAsync(Guid userId)
    {
        return context.Users.SingleOrDefaultAsync(x => x.Id == userId);
    }
}
