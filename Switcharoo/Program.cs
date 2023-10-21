using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;
using Switcharoo;
using Switcharoo.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IDbConnection>(_ => new SqliteConnection(builder.Configuration.GetConnectionString("SwitcharooDb")));
builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
builder.Services.AddScoped<IRepository, FeatureRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.VerifyDatabase(app.Services.GetRequiredService<IDbConnection>());

app.MapGet(
        "/{feature}/environment/{environmentKey}",
        async (Guid environmentKey, string feature, IFeatureProvider provider) => await provider.GetFeatureStateAsync(feature, environmentKey))
    .WithName("GetFeature").WithOpenApi();

app.MapPut(
        "/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.ToggleFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("ToggleFeature").WithOpenApi();

app.MapPost(
        "/feature/{authKey}",
        async (Guid authKey, string feature, string description, IFeatureProvider provider) => await provider.AddFeatureAsync(feature, description, authKey))
    .WithName("AddFeature").WithOpenApi();

app.MapPost(
        "/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.AddEnvironmentToFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("AddEnvironmentToFeature").WithOpenApi();

app.MapDelete(
        "/{featureKey}/{authKey}",
        async (Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.DeleteFeatureAsync(featureKey, authKey))
    .WithName("DeleteFeature").WithOpenApi();

app.MapDelete(
        "/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("DeleteEnvironmentFromFeature").WithOpenApi();

app.Run();

public static class WebApplicationExtensions
{
    public static void VerifyDatabase(this WebApplication app, IDbConnection connection)
    {
        var tables = (connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table'")).ToList();
        
        if (tables.Contains("Features"))
        {
            return; 
        }

        InitDatabase(connection);
    }

    private static void InitDatabase(IDbConnection connection)
    {
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Switcharoo.Database.db.sql");
        using var reader = new StreamReader(stream!);

        var sqlScript = reader.ReadToEnd();
        connection.Execute(sqlScript);
        
        connection.Execute("INSERT INTO Users (Name, authKey) VALUES (@Name, @authKey)", new {Name = "Admin", authKey = Guid.NewGuid()});
    }
}
