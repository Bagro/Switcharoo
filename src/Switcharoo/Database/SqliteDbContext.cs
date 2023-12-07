using Microsoft.EntityFrameworkCore;

namespace Switcharoo.Database;

public sealed class SqliteDbContext(DbContextOptions<SqliteDbContext> options) : BaseDbContext(options)
{
}
