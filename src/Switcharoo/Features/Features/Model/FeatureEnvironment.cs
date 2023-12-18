namespace Switcharoo.Features.Features.Model;

public sealed record FeatureEnvironment(bool IsEnabled, string EnvironmentName, Guid EnvironmentId);
