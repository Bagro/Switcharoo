using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.DeleteFeature;
using Xunit;

namespace Switcharoo.Tests.Features.Features.DeleteFeature;

public sealed class DeleteFeatureEndpointTests
{
    [Fact]
    public async Task HandleAsync_FeatureDeleted_ShouldReturnOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.DeleteFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((true, "Feature deleted"));
        
        // Act
        var result = await DeleteFeatureEndpoint.HandleAsync(Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNotFound_ShouldReturnBadRequestWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.DeleteFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((false, "Feature not found"));
        
        // Act
        var result = await DeleteFeatureEndpoint.HandleAsync(Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Feature not found");
    }
    
    [Fact]
    public async Task HandleAsync_ShouldCallRepository()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.DeleteFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((true, "Feature deleted"));
        
        // Act
        await DeleteFeatureEndpoint.HandleAsync(Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        await featureRepository.Received(1).DeleteFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>());
    }
}
