using Microsoft.EntityFrameworkCore;

namespace Switcharoo.Database;

public sealed class MariaDbContext(DbContextOptions<MariaDbContext> options) : BaseDbContext(options)
{
}
