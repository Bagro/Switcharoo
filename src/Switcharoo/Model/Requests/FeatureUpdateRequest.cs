namespace Switcharoo.Model.Requests;

public sealed record FeatureUpdateRequest(Guid Id, string Name, string Key, string Description, List<FeatureUpdateEnvironment> Environments);
