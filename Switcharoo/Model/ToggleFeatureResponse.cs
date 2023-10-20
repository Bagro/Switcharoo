namespace Switcharoo.Model;

public sealed record ToggleFeatureResponse(string FeatureName, bool IsActive, bool WasChanged, string Reason);
