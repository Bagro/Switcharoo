namespace Switcharoo.Entities;

public sealed class FeatureEnvironment
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public Feature Feature { get; set; }
    public Environment Environment { get; set; }
}
