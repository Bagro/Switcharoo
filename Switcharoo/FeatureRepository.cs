using System.Data;
using Dapper;
using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Switcharoo.Model;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo;

public sealed class FeatureRepository : IRepository
{
    private readonly IDbConnection _dbConnection;

    public FeatureRepository(IDbConnection connection)
    {
        _dbConnection = connection;
    }

    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        const string query = @"SELECT Active FROM FeatureEnvironments join Features on FeatureEnvironments.FeatureId = Features.Id join Environments on FeatureEnvironments.EnvironmentId = Environments.Id where Features.Name = @FeatureName and Environments.Key = @EnvironmentKey";

        var active = await _dbConnection.QuerySingleOrDefaultAsync<bool?>(query, new { FeatureName = featureName, EnvironmentKey = environmentKey.ToString().ToUpperInvariant() });

        return (active ?? false, active ?? false);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var featureId = await GetFeatureId(featureKey);
        if (featureId == 0)
        {
            return (false, false, "Feature not found");
        }
        
        const string featureStateQuery = @"SELECT Active FROM FeatureEnvironments WHERE FeatureId = @FeatureId AND EnvironmentId = (SELECT Id FROM Environments WHERE Key = @EnvironmentKey)";
        var featureState = await _dbConnection.QuerySingleAsync<bool>(featureStateQuery, new { FeatureId = featureId, EnvironmentKey = environmentKey.ToString().ToUpperInvariant() });

        const string updateQuery = @"UPDATE FeatureEnvironments SET Active = @Active WHERE FeatureId = (SELECT Id FROM Features WHERE Key = @FeatureKey) AND EnvironmentId = (SELECT Id FROM Environments WHERE Key = @EnvironmentKey)";
        await _dbConnection.ExecuteAsync(updateQuery, new { Active = !featureState, FeatureKey = featureKey.ToString().ToUpperInvariant(), EnvironmentKey = environmentKey.ToString().ToUpperInvariant() });

        return (!featureState, true, "Feature toggled");
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        var adminId = await GetAdminId(authKey);

        if (adminId == 0)
        {
            return (false, Guid.Empty, "User not found");
        }

        const string featureExistsQuery = @"SELECT Count(*) FROM Features WHERE Name = @Name AND UserId = @UserId";
        var featureExists = await _dbConnection.QuerySingleOrDefaultAsync<int>(featureExistsQuery, new { Name = featureName, UserId = adminId });
        if (featureExists > 0)
        {
            return (false, Guid.Empty, "Feature already exists");
        }

        const string insertQuery = @"INSERT INTO Features (Name, Key, Description, UserId) VALUES (@Name, @Key, @Description, @UserId)";
        var key = Guid.NewGuid();
        await _dbConnection.ExecuteAsync(insertQuery, new { Name = featureName, Key = key.ToString().ToUpperInvariant(), Description = description, UserId = adminId });

        return (true, key, "Feature added");
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var featureId = await GetFeatureId(featureKey);

        var environmentId = await GetEnvironmentId(environmentKey);

        if (featureId == 0 || environmentId == 0)
        {
            return (false, "Feature or environment not found");
        }

        const string featureEnvironmentExistsQuery = @"SELECT Count(*) FROM FeatureEnvironments WHERE FeatureId = @FeatureId AND EnvironmentId = @EnvironmentId";
        var featureEnvironmentExists = await _dbConnection.QuerySingleOrDefaultAsync<int>(featureEnvironmentExistsQuery, new { FeatureId = featureId, EnvironmentId = environmentId });
        if (featureEnvironmentExists > 0)
        {
            return (false, "Feature environment already exists");
        }

        const string insertQuery = @"INSERT INTO FeatureEnvironments (FeatureId, EnvironmentId, Active) VALUES (@FeatureId, @EnvironmentId, @Active)";
        await _dbConnection.ExecuteAsync(insertQuery, new { FeatureId = featureId, EnvironmentId = environmentId, Active = false });

        return (true, "Feature environment added");
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey)
    {
        var featureId = await GetFeatureId(featureKey);
        
        if (featureId == 0)
        {
            return (false, "Feature not found");
        }
        
        const string deleteEnvironmentQuery = @"DELETE FROM FeatureEnvironments WHERE FeatureId = @FeatureId";
        await _dbConnection.ExecuteAsync(deleteEnvironmentQuery, new { FeatureId = featureId });
        
        const string deleteQuery = @"DELETE FROM Features WHERE Id = @Id";
        await _dbConnection.ExecuteAsync(deleteQuery, new { Id = featureId });

        return (true, "Feature deleted");
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var featureId = await GetFeatureId(featureKey);
        var environmentId = await GetEnvironmentId(environmentKey);
        
        if (featureId == 0 || environmentId == 0)
        {
            return (false, "Feature or environment not found");
        }
        
        const string deleteQuery = @"DELETE FROM FeatureEnvironments WHERE FeatureId = @FeatureId AND EnvironmentId = @EnvironmentId";
        await _dbConnection.ExecuteAsync(deleteQuery, new { FeatureId = featureId, EnvironmentId = environmentId });
        
        return (true, "Feature environment deleted");
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid authKey)
    {
        var adminId = await GetAdminId(authKey);
        
        if (adminId == 0)
        {
            return (false, Guid.Empty, "User not found");
        }
        
        const string environmentExistsQuery = @"SELECT Count(*) FROM Environments WHERE Name = @Name AND UserId = @UserId";
        var environmentExists = await _dbConnection.QuerySingleOrDefaultAsync<int>(environmentExistsQuery, new { Name = environmentName, UserId = adminId });
        if (environmentExists > 0)
        {
            return (false, Guid.Empty, "Environment already exists");
        }
        
        const string insertQuery = @"INSERT INTO Environments (Name, Key, UserId) VALUES (@Name, @Key, @UserId)";
        var key = Guid.NewGuid();
        await _dbConnection.ExecuteAsync(insertQuery, new { Name = environmentName, Key = key.ToString().ToUpperInvariant(), UserId = adminId });
        
        return (true, key, "Environment added");
    }

    public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid authKey)
    {
        const string query = "SELECT Key, Name FROM Environments WHERE UserId = (SELECT Id FROM Users WHERE AuthKey = @AuthKey)";
        var result = (await _dbConnection.QueryAsync(query, new { AuthKey = authKey.ToString().ToUpperInvariant() })).ToList();

        var environments = new List<Environment>();
        foreach (var environment in result)
        {
            environments.Add(new Environment(new Guid(environment.Key), environment.Name));
        }
        
        return (environments.Any(), environments, environments.Any() ? "Environments found" : "No environments found"); 
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid authKey)
    {
        var isAdmin = await IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return (false, new List<Feature>(), "User not found");
        }
        
        const string featuresQuery = @"SELECT Id, Key, Name, Description FROM Features WHERE UserId = (SELECT Id FROM Users WHERE AuthKey = @AuthKey)";
        var featuresTask = _dbConnection.QueryAsync(featuresQuery, new { AuthKey = authKey.ToString().ToUpperInvariant() });
        
        const string environmentsQuery = "SELECT Id, Key, Name FROM Environments WHERE UserId = (SELECT Id FROM Users WHERE AuthKey = @AuthKey)";
        var environmentsTask = _dbConnection.QueryAsync(environmentsQuery, new { AuthKey = authKey.ToString().ToUpperInvariant() });
        
        await Task.WhenAll(featuresTask, environmentsTask);

        var foundFeatures = featuresTask.Result.ToList();
        var foundEnvironments = environmentsTask.Result.ToList();
        
        var features = new List<Feature>();
        
        foreach (var result in foundFeatures)
        {
            const string featureEnvironmentsQuery = @"SELECT EnvironmentId, Active FROM FeatureEnvironments WHERE  FeatureId = @FeatureId";
            var featureEnvironments = await _dbConnection.QueryAsync(featureEnvironmentsQuery, new { FeatureId = result.Id });

            var environments = new List<FeatureEnvironment>();
            
            foreach (var featureEnvironment in featureEnvironments)
            {
                var environment = foundEnvironments.SingleOrDefault(x => x.Id == featureEnvironment.EnvironmentId);
                if (environment == null)
                {
                    continue;
                }

                environments.Add(new FeatureEnvironment(new Guid(environment.Key), environment.Name.ToString(), Convert.ToBoolean(featureEnvironment.Active)));
            }
            
            var feature = new Feature(result.Name, result.Description, new Guid(result.Key), environments);   
            features.Add(feature);
        }

        return (true, features, string.Empty);
    }

    public async Task<bool> IsAdminAsync(Guid authKey)
    {
        const string query = @"SELECT * FROM Users WHERE AuthKey = @UserAuthKey";
        var result = await _dbConnection.QueryAsync(query, new { UserAuthKey = authKey.ToString().ToUpperInvariant() });

        return result.Any();
    }

    public async Task<bool> IsFeatureAdminAsync(Guid featureKey, Guid authKey)
    {
        const string query = @"SELECT * FROM Features WHERE Key = @FeatureKey AND UserId = (SELECT Id FROM Users WHERE AuthKey = @UserAuthKey)";
        var result = await _dbConnection.QueryAsync(query, new { FeatureKey = featureKey.ToString().ToUpperInvariant(), UserAuthKey = authKey.ToString().ToUpperInvariant() });
        
        return result.Any();
    }

    public async Task<bool> IsEnvironmentAdminAsync(Guid environmentKey, Guid authKey)
    {
        const string query = @"SELECT * FROM Environments WHERE Key = @EnvironmentKey AND UserId = (SELECT Id FROM Users WHERE AuthKey = @UserAuthKey)";
        var result = await _dbConnection.QueryAsync(query, new { EnvironmentKey = environmentKey.ToString().ToUpperInvariant(), UserAuthKey = authKey.ToString().ToUpperInvariant() });
        
        return result.Any();
    }

    private async Task<int> GetAdminId(Guid authKey)
    {
        const string adminIdQuery = @"SELECT Id FROM Users WHERE AuthKey = @AuthKey";
        var adminId = await _dbConnection.QuerySingleAsync<int>(adminIdQuery, new { AuthKey = authKey.ToString().ToUpperInvariant() });
        return adminId;
    }
    
    private async Task<int> GetEnvironmentId(Guid environmentKey)
    {
        const string environmentIdQuery = @"SELECT Id FROM Environments WHERE Key = @Key";
        var environmentId = await _dbConnection.QuerySingleAsync<int>(environmentIdQuery, new { Key = environmentKey.ToString().ToUpperInvariant() });
        return environmentId;
    }

    private async Task<int> GetFeatureId(Guid featureKey)
    {
        const string featureIdQuery = @"SELECT Id FROM Features WHERE Key = @Key";
        var featureId = await _dbConnection.QuerySingleAsync<int>(featureIdQuery, new { Key = featureKey.ToString().ToUpperInvariant() });
        return featureId;
    }
}
