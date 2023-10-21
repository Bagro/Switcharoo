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
        "/{key}/feature/{feature}",
        async (Guid key, string feature, IFeatureProvider provider) => await provider.GetFeatureStateAsync(feature, key))
    .WithName("GetFeature").WithOpenApi();

app.MapPut(
        "/{key}/feature/{feature}/{authkey}",
        async (Guid key, string feature, Guid authkey, IFeatureProvider provider) => await provider.ToggleFeatureAsync(feature, key, authkey))
    .WithName("ToggleFeature").WithOpenApi();

app.MapPost(
        "/{key}/feature/{authkey}",
        async (Guid key, Guid authkey, string feature, string description, IFeatureProvider provider) => await provider.AddFeatureAsync(feature, description, key, authkey))
    .WithName("AddFeature").WithOpenApi();

app.MapDelete(
        "/{key}/feature/{feature}/{authkey}",
        async (Guid key, string feature, Guid authkey, IFeatureProvider provider) => await provider.DeleteFeatureAsync(feature, key, authkey))
    .WithName("DeleteFeature").WithOpenApi();

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
        
        connection.Execute("INSERT INTO Users (Name, AuthKey) VALUES (@Name, @AuthKey)", new {Name = "Admin", AuthKey = Guid.NewGuid()});
    }
}
