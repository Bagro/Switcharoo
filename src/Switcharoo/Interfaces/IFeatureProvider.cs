using Switcharoo.Model;

namespace Switcharoo.Interfaces;

public interface IFeatureProvider
{
    Task<IResult> GetFeatureStateAsync(string featureName, Guid environmentKey);

    Task<IResult> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<IResult> AddFeatureAsync(string featureName, string description, Guid authKey);
    
    Task<IResult> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<IResult> DeleteFeatureAsync(Guid featureKey, Guid authKey);
    
    Task<IResult> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey);
    
    Task<IResult> AddEnvironmentAsync(string environmentName, Guid authKey);
    
    Task<IResult> GetEnvironmentsAsync(Guid authKey);
    
    Task<IResult> GetFeaturesAsync(Guid authKey);
}