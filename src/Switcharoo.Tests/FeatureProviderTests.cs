using FluentAssertions;
using NSubstitute;
using Switcharoo.Model;
using Switcharoo.Interfaces;
using Switcharoo.Providers;
using Xunit;

namespace Switcharoo.Tests;

public sealed class FeatureProviderTests
{
    private readonly IFeatureRepository _featureRepository;
    private readonly FeatureProvider _featureProvider;

    public FeatureProviderTests()
    {
        _featureRepository = Substitute.For<IFeatureRepository>();

        _featureProvider = new FeatureProvider(_featureRepository);
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
        var repository = Substitute.For<IFeatureRepository>();
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
        var repository = Substitute.For<IFeatureRepository>();
        repository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((false, true));
        var featureProvider = new FeatureProvider(repository);

        // Act
        var result = await featureProvider.GetFeatureStateAsync("featureKey", Guid.NewGuid());

        // Assert
        result.isActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetFeatureStateAsync_FeatureNotFound_ReturnsWasFoundFalse()
    {
        // Arrange
        var repository = Substitute.For<IFeatureRepository>();
        repository.GetFeatureStateAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((false, false));
        var featureProvider = new FeatureProvider(repository);

        // Act
        var result = await featureProvider.GetFeatureStateAsync("featureKey", Guid.NewGuid());

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
                Environments = new List<FeatureEnvironment> { new(true, "Environment 1", Guid.NewGuid()) },
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 2", Description = "Description 2", Key = "feature-2",
                Environments = new List<FeatureEnvironment> { new(true, "Environment 2", Guid.NewGuid()) },
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 3", Description = "Description 3", Key = "feature-3",
                Environments = new List<FeatureEnvironment> { new(true, "Environment 3", Guid.NewGuid()) },
            }
        };

        _featureRepository.GetFeaturesAsync(userId).Returns((true, features, string.Empty));

        return userId;
    }
}
