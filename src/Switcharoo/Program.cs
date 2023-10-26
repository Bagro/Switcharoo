using System.Data;
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
builder.Services.AddControllers();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.VerifyDatabase(app.Services.GetRequiredService<IDbConnection>());

app.MapControllers();

app.Run();
