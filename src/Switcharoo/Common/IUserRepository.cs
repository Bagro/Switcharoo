using Switcharoo.Database.Entities;

namespace Switcharoo.Common;

public interface IUserRepository
{
    Task<User?> GetUserAsync(Guid userId);
}
