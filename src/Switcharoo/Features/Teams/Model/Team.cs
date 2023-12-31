namespace Switcharoo.Features.Teams.Model;

public sealed record Team
{
    public Guid Id { get; init; } = Guid.Empty;
    
    public string Name { get; init; } = string.Empty;
    
    public string Description { get; init; } = string.Empty;
    
    public bool AllCanManage { get; init; }
    
    public bool InviteOnly { get; init; }
    
    /// <summary>
    /// Current members of the team. This is only shown for owner and members.
    /// </summary>
    public List<TeamMember> Members { get; init; } = new();
    
    public List<TeamFeature> Features { get; init; } = new();
    
    public List<TeamEnvironment> Environments { get; init; } = new();
}