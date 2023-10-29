using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class EnvironmentsController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet()]
    [ProducesResponseType<List<Environment>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnvironmentsAsync()
    {
        var result = await featureProvider.GetEnvironmentsAsync(User.GetUserId());

        return result.wasFound ? Ok(result.environments) : BadRequest(result.reason);
    }
}
