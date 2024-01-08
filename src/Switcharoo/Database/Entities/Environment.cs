namespace Switcharoo.Database.Entities;

public sealed class Environment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool ShareWithTeam { get; set; }
    public User Owner { get; set; }
    public List<FeatureEnvironment> Features { get; set; }
}
