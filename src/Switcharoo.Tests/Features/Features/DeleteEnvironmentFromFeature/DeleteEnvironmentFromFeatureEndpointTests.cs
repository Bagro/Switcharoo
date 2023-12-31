using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.DeleteEnvironmentFromFeature;
using Xunit;

namespace Switcharoo.Tests.Features.Features.DeleteEnvironmentFromFeature;

public sealed class DeleteEnvironmentFromFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var deleteEnvironmentFromFeatureEndpoint = new DeleteEnvironmentFromFeatureEndpoint();
        
        // Act
        deleteEnvironmentFromFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapDelete("/feature/{featureId}/environment/{environmentId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_CallsRepository()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        
        // Act
        await DeleteEnvironmentFromFeatureEndpoint.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        await featureRepository.Received(1).DeleteEnvironmentFromFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());
    }
    
    [Fact]
    public async Task HandleAsync_FeatureOrEnvironmentNotFound_ShouldReturnBadRequestWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.DeleteEnvironmentFromFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((false, "Feature not found"));
        // Act
        var result = await DeleteEnvironmentFromFeatureEndpoint.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Feature not found");
    }
    
    [Fact]
    public async Task HandleAsync_EnvironmentDeleted_ShouldReturnOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.DeleteEnvironmentFromFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((true, "Feature environment deleted"));
        // Act
        var result = await DeleteEnvironmentFromFeatureEndpoint.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok>();
    }
}
