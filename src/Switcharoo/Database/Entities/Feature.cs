namespace Switcharoo.Database.Entities;

public sealed record Feature
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public string Description { get; set; }
    public bool ShareWithTeam { get; set; }
    public User Owner { get; set; }
    public List<FeatureEnvironment> Environments { get; set; }
}