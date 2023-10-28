using System.Security.Authentication;
using System.Security.Claims;

namespace Switcharoo.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (claim == null)
        {
            throw new AuthenticationException("Unable to find user id");
        }

        return Guid.Parse(claim?.Value ?? Guid.Empty.ToString());
    }
}
