namespace Switcharoo.Model.Requests;

public sealed record AddTeamRequest(string Name, string Description, bool AllCanManage, bool InviteOnly);
