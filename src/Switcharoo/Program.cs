using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Switcharoo;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Features;
using Switcharoo.Interfaces;
using Switcharoo.Providers;
using Switcharoo.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

var httpOnly = builder.Configuration.GetSection("HTTP_Only").Get<bool?>() ?? false;

builder.Services.AddDatabase(builder.Configuration);

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

builder.Services.AddScoped<IEnvironmentProvider, EnvironmentProvider>();
builder.Services.AddScoped<ITeamProvider, TeamProvider>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

builder.Services.AddFeatures();

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

app.RegisterEndpoints();

app.MapControllers();

app.Run();
