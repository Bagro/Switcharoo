using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.GetFeatures;
using Switcharoo.Features.Features.Model;
using Xunit;
using Feature = Switcharoo.Features.Features.Model.Feature;

namespace Switcharoo.Tests.Features.Features.GetFeatures;

public sealed class GetFeaturesEndpointTests
{
    [Fact]
    public async Task HandleAsync_FeaturesExists_ShouldReturnFeatures()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();

        var features = new List<Switcharoo.Database.Entities.Feature>
        {
            new Switcharoo.Database.Entities.Feature
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
            },
        };
        
        featureRepository.GetFeaturesAsync(Arg.Any<Guid>()).Returns(features);
        
        var expectedFeatures = new List<Feature>()
        {
            new Feature
            {
                Id = features[0].Id,
                Name = features[0].Name,
                Key = features[0].Key,
                Description = features[0].Description,
                Environments = features[0].Environments.Select(y => new FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList()
            },
        };
        
        // Act
        var result = await GetFeaturesEndpoint.HandleAsync(user, featureRepository);
        
        // Assert
        result.Should().BeOfType<Ok<List<Feature>>>().Which.Value.Should().BeEquivalentTo(expectedFeatures);
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNotFound_ShouldReturnNotFoundWithCorrectReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.GetFeaturesAsync(Arg.Any<Guid>()).Returns(new List<Database.Entities.Feature>());
        
        // Act
        var result = await GetFeaturesEndpoint.HandleAsync(user, featureRepository);
        
        // Assert
        result.Should().BeOfType<NotFound<string>>().Which.Value.Should().Be("No features found");
    }
}
