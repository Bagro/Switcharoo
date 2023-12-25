using System.Security.Claims;

namespace Switcharoo.Tests;

public static class UserHelper
{
    public static ClaimsPrincipal GetClaimsPrincipalWithClaims()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Guid.NewGuid().ToString())
        }));
        return user;
    }
}
