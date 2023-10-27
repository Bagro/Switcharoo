using Microsoft.AspNetCore.Mvc;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

[ApiController]
[Route("[controller]")]
public class FeaturesController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{authKey}")]
    [ProducesResponseType<List<Feature>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeaturesAsync(Guid authKey)
    {
        var isAdmin = await featureProvider.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.GetFeaturesAsync(authKey);

        return result.wasFound ? Ok(result.features) : BadRequest(result.reason);
    }
}
