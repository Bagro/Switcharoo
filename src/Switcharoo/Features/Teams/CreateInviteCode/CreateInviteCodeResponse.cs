namespace Switcharoo.Features.Teams.CreateInviteCode;

public sealed record CreateInviteCodeResponse(Guid InviteCode, DateTimeOffset ExpiresAt, string TeamName, Guid TeamId);
