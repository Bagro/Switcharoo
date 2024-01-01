using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Switcharoo.Common;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Features;
using Switcharoo.Features.Teams;

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

builder.Services.AddOptions<SmtpSettings>().Bind(builder.Configuration.GetSection(nameof(SmtpSettings)));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddFeatures();
builder.Services.AddEnvironments();
builder.Services.AddTeams();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
