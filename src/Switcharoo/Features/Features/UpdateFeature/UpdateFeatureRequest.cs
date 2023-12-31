using Switcharoo.Features.Features.Model;

namespace Switcharoo.Features.Features.UpdateFeature;

public sealed record UpdateFeatureRequest(Guid Id, string Name, string Key, string Description, List<FeatureUpdateEnvironment> Environments);
