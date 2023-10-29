using FluentAssertions;
using NSubstitute;
using Switcharoo.Entities;
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
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsWasFound()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _featureProvider.GetEnvironmentsAsync(userId);

        // Assert
        result.wasFound.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsCorrectNumberOfEnvironments()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _featureProvider.GetEnvironmentsAsync(userId);

        // Assert
        result.environments.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsWithNoReason()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _featureProvider.GetEnvironmentsAsync(userId);

        // Assert
        result.reason.Should().BeEmpty();
    }

    private Guid GetEnvironmentsAsyncSetup()
    {
        var userId = Guid.NewGuid();

        var environments = new List<Entities.Environment>
        {
            new() { Id = Guid.NewGuid(), Name = "Environment 1", Owner = new Entities.User { Id = userId }, Features = new List<FeatureEnvironment>() },
            new() { Id = Guid.NewGuid(), Name = "Environment 2", Owner = new Entities.User { Id = userId }, Features = new List<FeatureEnvironment>() },
            new() { Id = Guid.NewGuid(), Name = "Environment 3", Owner = new Entities.User { Id = userId }, Features = new List<FeatureEnvironment>() },
        };

        _repository.GetEnvironmentsAsync(userId).Returns((true, environments, string.Empty));

        return userId;
    }

    private Guid GetFeaturesAsyncSetup()
    {
        var userId = Guid.NewGuid();

        var features = new List<Entities.Feature>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 1", Description = "Description 1", Owner = new Entities.User { Id = userId },
                Environments = new List<FeatureEnvironment> { new() { Environment = new Entities.Environment { Id = Guid.NewGuid(), Name = "Environment 1" }, IsEnabled = true } }
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 2", Description = "Description 2", Owner = new Entities.User { Id = userId },
                Environments = new List<FeatureEnvironment> { new() { Environment = new Entities.Environment { Id = Guid.NewGuid(), Name = "Environment 2" }, IsEnabled = true } },
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Feature 3", Description = "Description 3", Owner = new Entities.User { Id = userId },
                Environments = new List<FeatureEnvironment> { new() { Environment = new Entities.Environment { Id = Guid.NewGuid(), Name = "Environment 3" }, IsEnabled = true } },
            }
        };

        _repository.GetFeaturesAsync(userId).Returns((true, features, string.Empty));

        return userId;
    }
}
