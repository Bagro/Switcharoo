using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Switcharoo.Extensions;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

public sealed record AddEnvironmentRequest(string Name);

[ApiController]
[Authorize]
[Route("[controller]")]
public class EnvironmentController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpPost()]
    [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddEnvironmentAsync([FromBody] AddEnvironmentRequest request)
    {
        var result = await featureProvider.AddEnvironmentAsync(request.Name, User.GetUserId());

        return result.wasAdded ? Ok(new AddResponse(request.Name, result.key)) : BadRequest(result.reason);
    }
}