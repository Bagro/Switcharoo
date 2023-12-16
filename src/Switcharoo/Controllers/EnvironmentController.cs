using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.AddFeature;
using Switcharoo.Interfaces;
using Switcharoo.Model.Requests;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class EnvironmentController(IEnvironmentProvider environmentProvider) : ControllerBase
{
    
    [HttpGet("{id}")]
    [ProducesResponseType<Environment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnvironmentAsync(Guid id)
    {
        var result = await environmentProvider.GetEnvironmentAsync(id, User.GetUserId());
        
        return result.wasFound ? Ok(result.environment) : NotFound(result.reason);
    }
    
    [HttpPost]
    [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddEnvironmentAsync([FromBody] AddEnvironmentRequest request)
    {
        var result = await environmentProvider.AddEnvironmentAsync(request.Name, User.GetUserId());

        return result.wasAdded ? Ok(new AddResponse(request.Name, result.key)) : BadRequest(result.reason);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEnvironmentAsync([FromBody] Environment environment)
    {
        var result = await environmentProvider.UpdateEnvironmentAsync(environment, User.GetUserId());
        
        return result.wasUpdated ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEnvironmentAsync([FromBody] DeleteEnvironmentRequest request)
    {
        var result = await environmentProvider.DeleteEnvironmentAsync(request.EnvironmentId, User.GetUserId());
        
        return result.deleted ? Ok() : BadRequest();
    }
}