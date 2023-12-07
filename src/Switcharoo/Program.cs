using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Switcharoo;
using Switcharoo.Database;
using Switcharoo.Entities;
using Switcharoo.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

var httpOnly = builder.Configuration.GetSection("HTTP_Only").Get<bool?>() ?? false;

var dbType = builder.Configuration["DbType"];

if (dbType != null && dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<BaseDbContext, PostgresDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("SwitcharooDb")));
    builder.Services.AddIdentityCore<User>()
        .AddEntityFrameworkStores<PostgresDbContext>()
        .AddApiEndpoints();
}
else if (dbType != null && (dbType.Equals("MariaDB", StringComparison.OrdinalIgnoreCase) || dbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase)))
{
    var version = builder.Configuration["MyMariaVersion"];
    ServerVersion serverVersion = dbType.Equals("MariaDB", StringComparison.OrdinalIgnoreCase) ? new MariaDbServerVersion(version) : new MySqlServerVersion(version);
    builder.Services.AddDbContext<BaseDbContext, MariaDbContext>(x => x.UseMySql(builder.Configuration.GetConnectionString("SwitcharooDb"), serverVersion));
    builder.Services.AddIdentityCore<User>()
        .AddEntityFrameworkStores<MariaDbContext>()
        .AddApiEndpoints();
}
else
{
    builder.Services.AddDbContext<BaseDbContext, SqliteDbContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("SwitcharooDb")));
    builder.Services.AddIdentityCore<User>()
        .AddEntityFrameworkStores<SqliteDbContext>()
        .AddApiEndpoints();
}

builder.Services.AddCors(
    options => options.AddPolicy(
        "CorsPolicy",
        policyBuilder =>
        {
            var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string>()?.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (corsOrigins == null || corsOrigins.Length == 0)
            {
                return;
            }

            policyBuilder.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowedToAllowWildcardSubdomains();
        }));

builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
builder.Services.AddScoped<IEnvironmentProvider, EnvironmentProvider>();
builder.Services.AddScoped<IRepository, FeatureRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<RouteOptions>(
    options =>
    {
        options.LowercaseUrls = true;
    });

var app = builder.Build();
app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
app.UseSwagger();
#if DEBUG
app.UseSwaggerUI();
#endif

if (!httpOnly)
{
    app.UseHttpsRedirection();
}

app.MapGroup("auth").MapIdentityApi<User>();

app.MapControllers();

app.Run();
