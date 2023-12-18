namespace Switcharoo.Features.Teams.UpdateTeam;

public sealed record UpdateTeamRequest(Guid Id, string Name, string Description, bool AllCanManage, bool InviteOnly, List<UpdateTeamFeature> Features, List<UpdateTeamEnvironment> Environments);

public sealed record UpdateTeamFeature(Guid FeatureId, bool IsReadOnly, bool AllCanToggle);

public sealed record UpdateTeamEnvironment(Guid EnvironmentId, bool IsReadOnly);