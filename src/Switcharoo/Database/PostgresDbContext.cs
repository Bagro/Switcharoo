using Microsoft.EntityFrameworkCore;

namespace Switcharoo.Database;

public sealed class PostgresDbContext(DbContextOptions<PostgresDbContext> options) : BaseDbContext(options)
{
}
