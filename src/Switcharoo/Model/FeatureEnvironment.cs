namespace Switcharoo.Model;

public sealed class FeatureEnvironment
{
    public bool IsEnabled { get; set; }

    public string EnvironmentName { get; set; }

    public Guid EnvironmentId { get; set; }
}
