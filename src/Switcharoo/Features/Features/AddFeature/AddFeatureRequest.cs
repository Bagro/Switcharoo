﻿using Switcharoo.Model.Requests;

namespace Switcharoo.Features.Features.AddFeature;

public sealed record AddFeatureRequest(string Name, string Key, string Description, List<FeatureUpdateEnvironment>? Environments);
