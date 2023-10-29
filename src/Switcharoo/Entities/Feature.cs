namespace Switcharoo.Entities;

public sealed record Feature
{
    public Guid Id { get; set; }
    public string Name { get; set; } 
    public string Description { get; set; }
    public User Owner { get; set; }
    public List<FeatureEnvironment> Environments { get; set; }
}