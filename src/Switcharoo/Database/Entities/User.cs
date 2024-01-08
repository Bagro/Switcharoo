using Microsoft.AspNetCore.Identity;

namespace Switcharoo.Database.Entities;

public sealed class User : IdentityUser<Guid>
{
    public Team? Team { get; set; }

    public bool DefaultTeamAllowToggle { get; set; } = false;
    
    public bool DefaultTeamReadOnly { get; set; } = true;
}
