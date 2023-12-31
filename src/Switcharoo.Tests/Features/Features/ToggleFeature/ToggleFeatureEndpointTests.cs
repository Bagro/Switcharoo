using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Extensions;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.ToggleFeature;
using Xunit;

namespace Switcharoo.Tests.Features.Features.ToggleFeature;

public sealed class ToggleFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var toggleFeatureEndpoint = new ToggleFeatureEndpoint();
        
        // Act
        toggleFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPut("/feature/{featureId}/toggle", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNotFound_ShouldReturnBadRequestWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.ToggleFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), user.GetUserId()).Returns((false, false, "Feature not found"));
        
        // Act
        var result = await ToggleFeatureEndpoint.HandleAsync(new ToggleFeatureRequest(Guid.NewGuid(), Guid.NewGuid()), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Feature not found");
    }
    
    [Fact]
    public async Task HandleAsync_FeatureFound_ShouldReturnCorrectToggleFeatureResponse()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.ToggleFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), user.GetUserId()).Returns((true, true, "Feature toggled"));
        
        var featureId = Guid.NewGuid();
        // Act
        var result = await ToggleFeatureEndpoint.HandleAsync(new ToggleFeatureRequest(featureId, Guid.NewGuid()), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok<ToggleFeatureResponse>>().Which.Value.Should().BeEquivalentTo(new ToggleFeatureResponse(featureId.ToString(), true, true, "Feature toggled"));
    }
}
