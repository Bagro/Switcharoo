using Microsoft.AspNetCore.Mvc;
using Switcharoo.Interfaces;
using Switcharoo.Model;

namespace Switcharoo.Controllers;

public sealed record AddEnvironmentRequest(string Name, Guid AuthKey);

[ApiController]
[Route("[controller]")]
public class EnvironmentController(IFeatureProvider featureProvider) : ControllerBase
{
    [HttpPost()]
    [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddEnvironmentAsync([FromBody] AddEnvironmentRequest request)
    {
        var isAdmin = await featureProvider.IsAdminAsync(request.AuthKey);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await featureProvider.AddEnvironmentAsync(request.Name, request.AuthKey);

        return result.wasAdded ? Ok(new AddResponse(request.Name, result.key)) : BadRequest(result.reason);
    }
}
