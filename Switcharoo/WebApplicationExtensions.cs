using System.Data;
using System.Reflection;
using Dapper;

namespace Switcharoo;

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