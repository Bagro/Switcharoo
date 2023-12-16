namespace Switcharoo.Entities;

public sealed class Team
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public bool AllCanManage { get; set; }

    public bool InviteOnly { get; set; } = true;
    
    public User Owner { get; set; }
    
    public List<User> Members { get; set; }
    
    public List<TeamFeature> Features { get; set; }
    
    public List<TeamEnvironment> Environments { get; set; }
}
