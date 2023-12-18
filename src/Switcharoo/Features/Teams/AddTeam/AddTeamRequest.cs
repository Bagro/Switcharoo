namespace Switcharoo.Features.Teams.AddTeam;

public sealed record AddTeamRequest(string Name, string Description, bool AllCanManage, bool InviteOnly);
