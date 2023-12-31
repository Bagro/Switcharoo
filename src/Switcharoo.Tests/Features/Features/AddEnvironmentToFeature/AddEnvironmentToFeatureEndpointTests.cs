using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.AddEnvironmentToFeature;
using Xunit;

namespace Switcharoo.Tests.Features.Features.AddEnvironmentToFeature;

public sealed class AddEnvironmentToFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var addEnvironmentToFeatureEndpoint = new AddEnvironmentToFeatureEndpoint();
        
        // Act
        addEnvironmentToFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPost("/feature/{featureId}/environment/{environmentId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_InvalidFeatureIdOrEnvironmentId_ShouldReturnBadRequestWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.AddEnvironmentToFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((false, "Feature not found"));
        
        // Act
        var result = await AddEnvironmentToFeatureEndpoint.HandleAsync( new AddEnvironmentToFeatureRequest(Guid.NewGuid(), Guid.NewGuid()) ,user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Feature not found");
    }
    
    [Fact]
    public async Task HandleAsync_ValidFeatureIdAndEnvironmentId_ShouldReturnOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.AddEnvironmentToFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((true, string.Empty));
        
        // Act
        var result = await AddEnvironmentToFeatureEndpoint.HandleAsync( new AddEnvironmentToFeatureRequest(Guid.NewGuid(), Guid.NewGuid()) ,user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok>();
    }
}
