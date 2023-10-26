using System.Net;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Model.Environment;

namespace Switcharoo.Controllers;

[ApiController]
[Route("[controller]")]
public class EnvironmentsController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpGet("{authKey}")]
    [ProducesResponseType(typeof(List<Environment>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> GetEnvironmentsAsync(Guid authKey)
    {
        var isAdmin = await featureProvider.IsAdminAsync(authKey);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.GetEnvironmentsAsync(authKey);

        return result.wasFound ? Ok(result.environments) : BadRequest(result.reason);
    }
}
