namespace Switcharoo.Entities;

public sealed record User(int Id, Guid AuthKey, string Name);
