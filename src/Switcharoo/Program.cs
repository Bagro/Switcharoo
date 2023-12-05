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

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("SwitcharooDb")));

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

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

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
