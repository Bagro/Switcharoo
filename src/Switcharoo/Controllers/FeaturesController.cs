using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Entities;
using Switcharoo.Interfaces;

namespace Switcharoo.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class FeaturesController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{authKey}")]
    [ProducesResponseType<List<Feature>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeaturesAsync(Guid authKey)
    {
        var result = await featureProvider.GetFeaturesAsync(authKey);

        return result.wasFound ? Ok(result.features) : BadRequest(result.reason);
    }
}
