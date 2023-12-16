using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Team = Switcharoo.Model.Team;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public sealed class TeamsController(ITeamProvider teamProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<Team>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTeamsAsync()
    {
        var result = await teamProvider.GetTeamsAsync(User.GetUserId());
    
        return result.wasFound ? Ok(result.teams) : BadRequest(result.reason);
    }
}
