namespace Switcharoo.Model.Requests;

public sealed record DeleteEnvironmentFromFeatureRequest(Guid FeatureId, Guid EnvironmentId);
