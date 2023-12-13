namespace Switcharoo.Model.Requests;

public sealed record UpdateTeamRequest(Guid Id, string Name, string Description, bool AllCanManage, bool InviteOnly, List<UpdateTeamFeature> Features, List<UpdateTeamEnvironment> Environments);

public record UpdateTeamFeature(Guid FeatureId, bool IsReadOnly, bool AllCanToggle);
public record UpdateTeamEnvironment(Guid EnvironmentId, bool IsReadOnly);