using FluentAssertions;
using NSubstitute;
using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Xunit;

namespace Switcharoo.Tests;

public sealed class EnvironmentProviderTests
{
    private readonly EnvironmentProvider _environmentProvider;
    private readonly IRepository? _repository;

    public EnvironmentProviderTests()
    {
        _repository = Substitute.For<IRepository>();
        _environmentProvider = new EnvironmentProvider(_repository);
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsWasFound()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _environmentProvider.GetEnvironmentsAsync(userId);

        // Assert
        result.wasFound.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsCorrectNumberOfEnvironments()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _environmentProvider.GetEnvironmentsAsync(userId);

        // Assert
        result.environments.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_WhenEnvironmentsFound_ReturnsWithNoReason()
    {
        // Arrange
        var userId = GetEnvironmentsAsyncSetup();

        // Act
        var result = await _environmentProvider.GetEnvironmentsAsync(userId);

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

        _repository?.GetEnvironmentsAsync(userId).Returns((true, environments, string.Empty));

        return userId;
    }
}
