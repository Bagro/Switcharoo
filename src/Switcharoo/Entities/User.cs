using Microsoft.AspNetCore.Identity;

namespace Switcharoo.Entities;

public sealed class User : IdentityUser<Guid>
{
    public Team? Team { get; set; } 
}
