using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.GetFeatureState;
using Xunit;

namespace Switcharoo.Tests.Features.Features.GetFeatureState;

public sealed class GetFeatureStateEndpointTests
{
    [Fact]
    public async Task HandleAsync_FeatureExists_ShouldReturnFeatureState()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns((true, true));
        
        // Act
        var result = await GetFeatureStateEndpoint.HandleAsync("test-feature", Guid.NewGuid(), featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok<bool>>().Which.Value.Should().BeTrue();
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns((false, false));
        
        // Act
        var result = await GetFeatureStateEndpoint.HandleAsync("test-feature", Guid.NewGuid(), featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<NotFound>();
    }
}
