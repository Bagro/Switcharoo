using System.Net;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

public sealed record ToggleFeatureRequest(Guid FeatureKey, Guid EnvironmentKey, Guid AuthKey);
public sealed record AddFeatureRequest(string Name, string Description, Guid AuthKey);
public sealed record AddEnvironmentToFeatureRequest(Guid FeatureKey, Guid EnvironmentKey, Guid AuthKey);
public sealed record DeleteFeatureRequest(Guid FeatureKey, Guid AuthKey);
public sealed record DeleteEnvironmentFromFeatureRequest(Guid FeatureKey, Guid EnvironmentKey, Guid AuthKey);

[ApiController]
[Route("[controller]")]
public sealed class FeatureController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{featureName}/environment/{environmentKey}")]
    [ProducesResponseType(typeof(FeatureStateResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetFeatureAsync(string featureName, Guid environmentKey)
    {
        var result = await featureProvider.GetFeatureStateAsync(featureName, environmentKey);
        
        return result.wasFound ? Ok(new FeatureStateResponse(featureName, result.isActive)) : NotFound();
    }
    
    [HttpPut()]
    [ProducesResponseType(typeof(ToggleFeatureResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> ToggleFeature([FromBody] ToggleFeatureRequest request)
    {
        var isAdmin = await featureProvider.IsAdminAsync(request.AuthKey, request.EnvironmentKey, request.FeatureKey);
        if (!isAdmin)
        {
            return Forbid();
        }
        
        var result = await featureProvider.ToggleFeatureAsync(request.FeatureKey, request.EnvironmentKey, request.AuthKey);
        
        return result.wasChanged ? Ok(new ToggleFeatureResponse(request.FeatureKey.ToString(), result.isActive, result.wasChanged, result.reason)) : Forbid();
    }
    
    [HttpPost()]
    [ProducesResponseType(typeof(AddResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> AddFeatureAsync([FromBody] AddFeatureRequest request)
    {
        var isAdmin = await featureProvider.IsAdminAsync(request.AuthKey);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.AddFeatureAsync(request.Name, request.Description, request.AuthKey);

        return result.wasAdded ? Ok(new AddResponse(request.Name, result.key)) : BadRequest(result.reason);
    }
    
    [HttpPost("environment")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> AddEnvironmentToFeatureAsync([FromBody] AddEnvironmentToFeatureRequest request)
    {
        var isAdmin = await featureProvider.IsAdminAsync(request.AuthKey);
     
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.AddEnvironmentToFeatureAsync(request.FeatureKey, request.EnvironmentKey);

        return result.wasAdded ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> DeleteFeatureAsync([FromBody] DeleteFeatureRequest request)
    {
        var isAdmin = await featureProvider.IsFeatureAdminAsync(request.AuthKey, request.FeatureKey);
     
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.DeleteFeatureAsync(request.FeatureKey);

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("environment")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> DeleteEnvironmentFromFeatureAsync([FromBody] DeleteEnvironmentFromFeatureRequest request)
    {
        var isAdmin = await featureProvider.IsAdminAsync(request.AuthKey, request.EnvironmentKey, request.FeatureKey);
     
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.DeleteEnvironmentFromFeatureAsync(request.FeatureKey, request.EnvironmentKey);

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
}
