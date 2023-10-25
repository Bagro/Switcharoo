using System.Data;
using System.Net;
using Microsoft.Data.Sqlite;
using Switcharoo;
using Switcharoo.Interfaces;
using Switcharoo.Model;
using Environment = Switcharoo.Model.Environment;

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
        "/environments/{authKey}",
        async (Guid authKey, IFeatureProvider provider) => await provider.GetEnvironmentsAsync(authKey))
    .WithName("GetEnvironments").WithOpenApi()
    .Produces<List<Environment>>().Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapPost(
        "/environment/{authKey}",
        async (Guid authKey, string environmentName, IFeatureProvider provider) => await provider.AddEnvironmentAsync(environmentName, authKey))
    .WithName("AddEnvironment").WithOpenApi().Produces<AddResponse>().Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapGet(
        "/features/{authKey}",
        async (Guid authKey, IFeatureProvider provider) => await provider.GetFeaturesAsync(authKey))
    .WithName("GetFeatures").WithOpenApi().Produces<List<Feature>>().Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapGet(
        "/feature/{featureName}/environment/{environmentKey}",
        async (Guid environmentKey, string featureName, IFeatureProvider provider) => await provider.GetFeatureStateAsync(featureName, environmentKey))
    .WithName("GetFeature").WithOpenApi()
    .Produces<FeatureStateResponse>().Produces((int)HttpStatusCode.NotFound, typeof(string));

app.MapPut(
        "/feature/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.ToggleFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("ToggleFeature").WithOpenApi().Produces<ToggleFeatureResponse>().Produces((int)HttpStatusCode.Forbidden);

app.MapPost(
        "/feature/{authKey}",
        async (Guid authKey, string feature, string description, IFeatureProvider provider) => await provider.AddFeatureAsync(feature, description, authKey))
    .WithName("AddFeature").WithOpenApi().Produces<AddResponse>().Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapPost(
        "/feature/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.AddEnvironmentToFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("AddEnvironmentToFeature").WithOpenApi().Produces((int)HttpStatusCode.OK).Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapDelete(
        "/feature/{featureKey}/{authKey}",
        async (Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.DeleteFeatureAsync(featureKey, authKey))
    .WithName("DeleteFeature").WithOpenApi().Produces((int)HttpStatusCode.OK).Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.MapDelete(
        "/feature/{featureKey}/environment/{environmentKey}/{authKey}",
        async (Guid environmentKey, Guid featureKey, Guid authKey, IFeatureProvider provider) => await provider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey, authKey))
    .WithName("DeleteEnvironmentFromFeature").WithOpenApi().Produces((int)HttpStatusCode.OK).Produces((int)HttpStatusCode.BadRequest, typeof(string)).Produces((int)HttpStatusCode.Forbidden);

app.Run();
