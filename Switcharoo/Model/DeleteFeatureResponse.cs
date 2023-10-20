namespace Switcharoo.Model;

public sealed record DeleteFeatureResponse(string FeatureName, bool WasDeleted, string? ErrorMessage = null);
