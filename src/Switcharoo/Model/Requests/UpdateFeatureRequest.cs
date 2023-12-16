namespace Switcharoo.Model.Requests;

public sealed record UpdateFeatureRequest(Guid Id, string Name, string Key, string Description, List<FeatureUpdateEnvironment> Environments);
