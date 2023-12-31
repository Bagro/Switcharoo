namespace Switcharoo.Features.Features.ToggleFeature;

public sealed record ToggleFeatureResponse(string FeatureName, bool IsActive, bool WasChanged, string Reason);
