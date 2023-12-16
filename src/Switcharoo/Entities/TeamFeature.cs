namespace Switcharoo.Entities;

public sealed class TeamFeature
{
    public Guid Id { get; set; }
    
    public bool IsReadOnly { get; set; }

    public bool AllCanToggle { get; set; }
    
    public Team Team { get; set; }
    
    public Feature Feature { get; set; }
}
