namespace Switcharoo.Model.Requests;

public sealed record AddEnvironmentToFeatureRequest(Guid FeatureId, Guid EnvironmentId);
