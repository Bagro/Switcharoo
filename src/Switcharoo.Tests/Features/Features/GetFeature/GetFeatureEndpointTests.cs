using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.GetFeature;
using Switcharoo.Features.Features.Model;
using Xunit;


namespace Switcharoo.Tests.Features.Features.GetFeature;

public sealed class GetFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var getFeatureEndpoint = new GetFeatureEndpoint();
        
        // Act
        getFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapGet("/feature/{featureId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_FeatureExists_ShouldReturnFeature()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();

        var feature = new Switcharoo.Database.Entities.Feature
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Key = "Test",
            Description = "Test",
            Environments =
            [
                new Switcharoo.Database.Entities.FeatureEnvironment
                {
                    Id = Guid.NewGuid(),
                    Environment = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test",
                    },
                    IsEnabled = true
                },
            ],
        };
        
        featureRepository.GetFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(feature);
        
        var expectedFeature = new Feature
        {
            Id = feature.Id,
            Name = feature.Name,
            Key = feature.Key,
            Description = feature.Description,
            Environments = feature.Environments.Select(y => new FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList()
        };

        // Act
        var result = await GetFeatureEndpoint.HandleAsync(Guid.NewGuid(), user, featureRepository, CancellationToken.None);

        // Assert
        ((Ok<Feature>)result).Value.Should().BeEquivalentTo(expectedFeature);
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNotFound_ShouldReturnNotFoundWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.GetFeatureAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((Switcharoo.Database.Entities.Feature?)null);
        
        // Act
        var result = await GetFeatureEndpoint.HandleAsync(Guid.NewGuid(), user, featureRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<NotFound<string>>().Which.Value.Should().Be("Feature not found");
    }
    
    
}
