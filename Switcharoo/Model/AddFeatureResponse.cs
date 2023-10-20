namespace Switcharoo.Model;

public sealed record AddFeatureResponse(string FeatureName, bool WasAdded, string? ErrorMessage = null);
