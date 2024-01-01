namespace Switcharoo.Features.Teams.CreateInviteCode;

public sealed record CreateInviteCodeRequest(Guid TeamId, int ValidForDays = 30);
