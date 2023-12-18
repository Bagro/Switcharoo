using Switcharoo.Database.Entities;

namespace Switcharoo.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(Guid userId);
}
