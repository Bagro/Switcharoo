using System.Data;
using Dapper;
using Switcharoo.Interfaces;

namespace Switcharoo;

public sealed class FeatureRepository : IRepository
{
    private readonly IDbConnection _dbConnection;

    public FeatureRepository(IDbConnection connection)
    {
        _dbConnection = connection;
    }
    
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid key)
    {
        const string query = $"SELECT Active FROM Features WHERE Name = @Name AND EnvironmentKey = @Key";

        var active = await _dbConnection.QuerySingleOrDefaultAsync<bool?>(query, new { Name = featureName, Key = key.ToString() });
        
        return (active ?? false, active ?? false);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(string featureName, Guid key, Guid authKey)
    {
        var featureState = await GetFeatureStateAsync(featureName, key);
        if (!featureState.wasFound)
        {
            return (false, false, "Feature not found");
        }
        
        const string udateQuery = @"UPDATE Features SET Active = @Active WHERE Name = @Name AND EnvironmentKey = @Key";
        await _dbConnection.ExecuteAsync(udateQuery, new { Active = !featureState.isActive, Name = featureName, Key = key.ToString() });
        
        return (!featureState.isActive, true, "Feature toggled");
    }

    public async Task<(bool wasAdded, string reason)> AddFeatureAsync(string featureName, string description, Guid key, Guid authKey)
    {
        if((await GetFeatureStateAsync(featureName, key)).wasFound)
        {
            return (false, "Feature already exists");
        }
        
        const string insertQuery = @"INSERT INTO Features (Name, EnvironmentKey, Description, Active) VALUES (@Name, @Key, @Description, @Active)";
        await _dbConnection.ExecuteAsync(insertQuery, new { Name = featureName, Key = key.ToString(), Description = description, Active = false });
        
        return (true, "Feature added");
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(string featureName, Guid key, Guid authKey)
    {
        const string deleteQuery = @"DELETE FROM Features WHERE Name = @Name AND EnvironmentKey = @Key";
        await _dbConnection.ExecuteAsync(deleteQuery, new { Name = featureName, Key = key.ToString() });
        
        return (true, "Feature deleted");
    }

    public async Task<bool> IsAdminAsync(Guid key, Guid authKey)
    {
        const string query = @"SELECT * FROM UserEnvironment WHERE EnvironmentKey = @Key AND UserAuthKey = @UserAuthKey";
        var result = await _dbConnection.QueryAsync(query, new { Key = key.ToString(), UserAuthKey = authKey.ToString() });
        
        return result.Any();
    }
}
