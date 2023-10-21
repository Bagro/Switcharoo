namespace Switcharoo.Model;

public sealed record AddFeatureResponse(string FeatureName, Guid Key, bool WasAdded, string? ErrorMessage = null);
