namespace Switcharoo.Database.Entities;

public sealed class TeamEnvironment
{
    public Guid Id { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    public Team Team { get; set; }
    
    public Environment Environment { get; set; }
}
