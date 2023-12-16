namespace Switcharoo.Entities;

public sealed class TeamInvite
{
    public Guid InviteCode { get; set; }
    
    public Team Team { get; set; }
    
    public User InvitedBy { get; set; }
    
    public User? ActivatedByUser { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? ActivatedAt { get; set; }
}
