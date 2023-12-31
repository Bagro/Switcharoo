using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Database.Entities;

namespace Switcharoo.Database;

public static class ServiceExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dbType = configuration["DbType"];

        if (dbType != null && dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<BaseDbContext, PostgresDbContext>(x => x.UseNpgsql(configuration.GetConnectionString("SwitcharooDb")));
            services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<PostgresDbContext>()
                .AddApiEndpoints();
        }
        else if (dbType != null && (dbType.Equals("MariaDB", StringComparison.OrdinalIgnoreCase) || dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase)))
        {
            var version = configuration["MyMariaVersion"];
            ServerVersion serverVersion = dbType.Equals("MariaDB", StringComparison.OrdinalIgnoreCase) ? new MariaDbServerVersion(version) : new MySqlServerVersion(version);
            services.AddDbContext<BaseDbContext, MariaDbContext>(x => x.UseMySql(configuration.GetConnectionString("SwitcharooDb"), serverVersion));
            services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<MariaDbContext>()
                .AddApiEndpoints();
        }
        else
        {
            services.AddDbContext<BaseDbContext, SqliteDbContext>(x => x.UseSqlite(configuration.GetConnectionString("SwitcharooDb")));
            services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<SqliteDbContext>()
                .AddApiEndpoints();
        }
    }
}
