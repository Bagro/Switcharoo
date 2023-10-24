namespace Switcharoo.Model;

public sealed record FeatureEnvironment(Guid EnvironmentKey, string Name, bool IsEnabled);
