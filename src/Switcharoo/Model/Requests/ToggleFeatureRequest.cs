namespace Switcharoo.Model.Requests;

public sealed record ToggleFeatureRequest(Guid FeatureId, Guid EnvironmentId);
