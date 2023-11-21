namespace Switcharoo.Model;

public sealed class Feature
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public string Description { get; set; }
    public List<FeatureEnvironment> Environments { get; set; }
}
