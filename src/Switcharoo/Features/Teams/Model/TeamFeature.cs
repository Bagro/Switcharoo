namespace Switcharoo.Features.Teams.Model;

public sealed record TeamFeature(Guid Id, string Name, bool IsReadOnly, bool AllCanToggle);
