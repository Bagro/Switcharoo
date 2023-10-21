namespace Switcharoo.Model;

public sealed record DeleteFeatureResponse(bool WasDeleted, string? ErrorMessage = null);
