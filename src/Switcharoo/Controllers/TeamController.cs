using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model.Requests;
using Switcharoo.Model.Responses;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public sealed class TeamController(ITeamProvider teamProvider) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTeamAsync([FromBody] AddTeamRequest request)
    {
        var result = await teamProvider.AddTeamAsync(request, User.GetUserId());
        
        return result.wasAdded ? Ok(new AddResponse(request.Name, result.id)) : BadRequest(result.reason);
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
}
