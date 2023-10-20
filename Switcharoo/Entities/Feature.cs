namespace Switcharoo.Entities;

public sealed record Feature(int Id, string Name, string Description, bool Active, Guid EnvironmentKey);
