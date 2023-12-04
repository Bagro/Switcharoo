using FluentAssertions;
using NSubstitute;
using Switcharoo.Model;
using Switcharoo.Interfaces;
using Xunit;

namespace Switcharoo.Tests;

public sealed class FeatureProviderTests
{
    private readonly IRepository _repository;
    private readonly FeatureProvider _featureProvider;

    public FeatureProviderTests()
    {
        _repository = Substitute.For<IRepository>();

        _featureProvider = new FeatureProvider(_repository);
    }

    [Fact]
    public async Task GetFeaturesAsync_WhenFeaturesFound_ReturnsWasFound()
    {
        // Arrange
        var userId = GetFeaturesAsyncSetup();

        // Act
        var result = await _featureProvider.GetFeaturesAsync(userId);

        // Assert
        result.wasFound.Should().BeTrue();
    }

    [Fact]
    public async Task GetFeaturesAsync_WhenFeaturesFound_ReturnsCorrectNumberOfFeatures()
    {
        // Arrange
        var userId = GetFeaturesAsyncSetup();

        // Act
        var result = await _featureProvider.GetFeaturesAsync(userId);

        // Assert
        result.features.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFeaturesAsync_WhenFeaturesFound_ReturnsWithNoReason()
    {
        // Arrange
        var userId = GetFeaturesAsyncSetup();

        // Act
        var result = await _featureProvider.GetFeaturesAsync(userId);

        // Assert
        result.reason.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetFeatureStateAsync_ActiveFeature_ReturnsIsActiveTrue()
    {
        // Arrange
        var repository = Substitute.For<IRepository>();
        repository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((true, true));
        var sut = new FeatureProvider(repository);
            
        // Act
        var result = await sut.GetFeatureStateAsync("featureKey", Guid.NewGuid());
            
        // Assert
        result.isActive.Should().BeTrue();
    }
        
    [Fact]
    public async Task GetFeatureStateAsync_InactiveFeature_ReturnsIsActiveFalse()
    {
        // Arrange
        var repository = Substitute.For<IRepository>();
        repository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((false, true));
        var sut = new FeatureProvider(repository);
            
        // Act
        var result = await sut.GetFeatureStateAsync("featureKey", Guid.NewGuid());
            
        // Assert
        result.isActive.Should().BeFalse();
    }
        
    [Fact]
    public async Task GetFeatureStateAsync_FeatureNotFound_ReturnsWasFoundFalse()
    {
        // Arrange
        var repository = Substitute.For<IRepository>();
        repository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((false, false));
        var sut = new FeatureProvider(repository);
            
        // Act
        var result = await sut.GetFeatureStateAsync("featureKey", Guid.NewGuid());
            
        // Assert
        result.wasFound.Should().BeFalse();
    }

    private Guid GetFeaturesAsyncSetup()
    {
        var userId = Guid.NewGuid();

        var features = new List<Feature>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 1", Description = "Description 1", Key = "feature-1",
                Environments = new List<FeatureEnvironment> { new() {  EnvironmentId = Guid.NewGuid(), EnvironmentName = "Environment 1" , IsEnabled = true } },
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 2", Description = "Description 2", Key = "feature-2",
                Environments = new List<FeatureEnvironment> { new() { EnvironmentId = Guid.NewGuid(), EnvironmentName = "Environment 2" , IsEnabled = true } },
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 3", Description = "Description 3", Key = "feature-3",
                Environments = new List<FeatureEnvironment> { new() { EnvironmentId = Guid.NewGuid(), EnvironmentName = "Environment 3" , IsEnabled = true } },
            }
        };

        _repository.GetFeaturesAsync(userId).Returns((true, features, string.Empty));

        return userId;
    }
}
