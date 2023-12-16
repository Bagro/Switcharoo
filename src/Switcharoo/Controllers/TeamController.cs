using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.AddFeature;
using Switcharoo.Interfaces;
using Switcharoo.Model;
using Switcharoo.Model.Requests;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public sealed class TeamController(ITeamProvider teamProvider) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AddFeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTeamAsync([FromBody] AddTeamRequest request)
    {
        var result = await teamProvider.AddTeamAsync(request, User.GetUserId());
        
        return result.wasAdded ? Ok(new AddFeatureResponse(request.Name, result.id)) : BadRequest(result.reason);
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTeamAsync([FromBody] UpdateTeamRequest request)
    {
        var result = await teamProvider.UpdateTeamAsync(request, User.GetUserId());
        
        return result.wasUpdated ? Ok() : BadRequest(result.reason);
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTeamAsync([FromBody] DeleteTeamRequest request)
    {
        var result = await teamProvider.DeleteTeamAsync(request, User.GetUserId());
        
        return result.wasDeleted ? Ok() : BadRequest(result.reason);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Team>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTeamAsync(Guid id)
    {
        var result = await teamProvider.GetTeamAsync(id, User.GetUserId());
        
        return result.wasFound ? Ok(result.team) : NotFound(result.reason);
    }
}
