using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

public sealed record ToggleFeatureRequest(Guid FeatureId, Guid EnvironmentId);
public sealed record AddFeatureRequest(string Name, string Description);
public sealed record AddEnvironmentToFeatureRequest(Guid FeatureId, Guid EnvironmentId);
public sealed record DeleteFeatureRequest(Guid FeatureId);
public sealed record DeleteEnvironmentFromFeatureRequest(Guid FeatureId, Guid EnvironmentId);

[ApiController]
[Authorize]
[Route("[controller]")]
public sealed class FeatureController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{featureName}/environment/{environmentId}")]
    [AllowAnonymous]
    [ProducesResponseType<FeatureStateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeatureAsync(string featureName, Guid environmentId)
    {
        var result = await featureProvider.GetFeatureStateAsync(featureName, environmentId);
        
        return result.wasFound ? Ok(new FeatureStateResponse(featureName, result.isActive)) : NotFound();
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType<Feature>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeatureAsync(Guid id)
    {
        var result = await featureProvider.GetFeatureAsync(id, User.GetUserId());
        
        return result.wasFound ? Ok(result.feature) : NotFound(result.reason);
    }
    
    [HttpPut("toggle")]
    [ProducesResponseType<ToggleFeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleFeature([FromBody] ToggleFeatureRequest request)
    {
        var result = await featureProvider.ToggleFeatureAsync(request.FeatureId, request.EnvironmentId, User.GetUserId());
        
        return result.wasChanged ? Ok(new ToggleFeatureResponse(request.FeatureId.ToString(), result.isActive, result.wasChanged, result.reason)) : Forbid();
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateFeatureAsync([FromBody] Feature feature)
    {
        var result = await featureProvider.UpdateFeatureAsync(feature, User.GetUserId());
        
        return result.wasUpdated ? Ok() : BadRequest(result.reason);
    }
    
    [HttpPost]
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
        var result = await featureProvider.AddEnvironmentToFeatureAsync(request.FeatureId, request.EnvironmentId, User.GetUserId());

        return result.wasAdded ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFeatureAsync([FromBody] DeleteFeatureRequest request)
    {
        var result = await featureProvider.DeleteFeatureAsync(request.FeatureId, User.GetUserId());

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete("environment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEnvironmentFromFeatureAsync([FromBody] DeleteEnvironmentFromFeatureRequest request)
    {
        var result = await featureProvider.DeleteEnvironmentFromFeatureAsync(request.FeatureId, request.EnvironmentId, User.GetUserId());

        return result.deleted ? Ok() : BadRequest(result.reason);
    }
}
