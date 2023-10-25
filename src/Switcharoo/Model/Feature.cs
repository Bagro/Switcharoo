namespace Switcharoo.Model;

public sealed record Feature(string Name, string Description, Guid Key, IEnumerable<FeatureEnvironment> Environments);
