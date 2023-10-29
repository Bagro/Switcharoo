using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Entities;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class FeaturesController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("")]
    [ProducesResponseType<List<Feature>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeaturesAsync()
    {
        var result = await featureProvider.GetFeaturesAsync(User.GetUserId());

        return result.wasFound ? Ok(result.features) : NotFound(result.reason);
    }
}
