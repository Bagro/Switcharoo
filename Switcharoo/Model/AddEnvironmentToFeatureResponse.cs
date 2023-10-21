namespace Switcharoo.Model;

public sealed record AddEnvironmentToFeatureResponse(bool WasAdded, string? ErrorMessage = null);
