namespace Switcharoo.Model.Requests;

public sealed record AddFeatureRequest(string Name, string Key, string Description, List<FeatureUpdateEnvironment> Environments);
