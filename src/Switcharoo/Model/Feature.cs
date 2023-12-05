namespace Switcharoo.Model;

public sealed class Feature
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
     
    public List<FeatureEnvironment> Environments { get; set; } = new List<FeatureEnvironment>();
}
