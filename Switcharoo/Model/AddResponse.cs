namespace Switcharoo.Model;

public sealed record AddResponse(string Name, Guid Key, bool WasAdded, string Reason);
