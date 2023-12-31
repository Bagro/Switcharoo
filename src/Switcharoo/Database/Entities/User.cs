using Microsoft.AspNetCore.Identity;

namespace Switcharoo.Database.Entities;

public sealed class User : IdentityUser<Guid>
{
    public Team? Team { get; set; } 
}
