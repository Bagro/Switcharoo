using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

public sealed record ToggleFeatureRequest(Guid FeatureKey, Guid EnvironmentKey);
public sealed record AddFeatureRequest(string Name, string Description);
public sealed record AddEnvironmentToFeatureRequest(Guid FeatureKey, Guid EnvironmentKey);
public sealed record DeleteFeatureRequest(Guid FeatureKey);
public sealed record DeleteEnvironmentFromFeatureRequest(Guid FeatureKey, Guid EnvironmentKey);

[ApiController]
[Authorize]
[Route("[controller]")]
public sealed class FeatureController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{featureName}/environment/{environmentKey}")]
    [AllowAnonymous]
    [ProducesResponseType<FeatureStateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeatureAsync(string featureName, Guid environmentKey)
    {
        var result = await featureProvider.GetFeatureStateAsync(featureName, environmentKey);
        
        return result.wasFound ? Ok(new FeatureStateResponse(featureName, result.isActive)) : NotFound();
    }
    
    [HttpPut()]
    [ProducesResponseType<ToggleFeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleFeature([FromBody] ToggleFeatureRequest request)
    {
        var result = await featureProvider.ToggleFeatureAsync(request.FeatureKey, request.EnvironmentKey, User.GetUserId());
        
        return result.wasChanged ? Ok(new ToggleFeatureResponse(request.FeatureKey.ToString(), result.isActive, result.wasChanged, result.reason)) : Forbid();
    }
    
    [HttpPost()]
    [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddFeatureAsync([FromBody] AddFeatureRequest request)
    {
        var result = await featureProvider.AddFeatureAsync(request.Name, request.Description, User.GetUserId());

        return result.wasAdded ? Ok(new AddResponse(request.Name, result.key)) : BadRequest(result.reason);
    }
    
    [HttpPost("environment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddEnvironmentToFeatureAsync([FromBody] AddEnvironmentToFeatureRequest request)
    {
        var result = await featureProvider.AddEnvironmentToFeatureAsync(request.FeatureKey, request.EnvironmentKey);

        return result.wasAdded ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFeatureAsync([FromBody] DeleteFeatureRequest request)
    {
        var result = await featureProvider.DeleteFeatureAsync(request.FeatureKey);

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("environment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEnvironmentFromFeatureAsync([FromBody] DeleteEnvironmentFromFeatureRequest request)
    {
        var result = await featureProvider.DeleteEnvironmentFromFeatureAsync(request.FeatureKey, request.EnvironmentKey);

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
}
