using FluentAssertions;
using NSubstitute;
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
    public async Task GetFeatureStateAsync_WhenCalled_ReturnsFeatureStateResponse()
    {
        // Arrange
        const string featureName = "TestFeature";
        var environmentKey = Guid.NewGuid();
        _repository.GetFeatureStateAsync(featureName, environmentKey).Returns((true, true));

        // Act
        var result = await _featureProvider.GetFeatureStateAsync(featureName, environmentKey);

        // Assert
        result.isActive.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetFeatureStateAsync_WhenFeatureNotFound_ReturnsNotFound()
    {
        // Arrange
        const string featureName = "TestFeature";
        var environmentKey = Guid.NewGuid();
        _repository.GetFeatureStateAsync(featureName, environmentKey).Returns((false, false));

        // Act
        var result = await _featureProvider.GetFeatureStateAsync(featureName, environmentKey);

        // Assert
        result.wasFound.Should().BeFalse();
    }
   
    // [Fact]
    // public async Task ToggleFeatureAsync_WhenCalled_ReturnsToggleFeatureResponse()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     var toggleFeatureResponse = new ToggleFeatureResponse(featureKey.ToString(), true, true, "Feature toggled");
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(true);
    //     _repository.ToggleFeatureAsync(featureKey, environmentKey, authKey).Returns((toggleFeatureResponse.IsActive, toggleFeatureResponse.WasChanged, toggleFeatureResponse.Reason));
    //
    //     // Act
    //     var result = await _featureProvider.ToggleFeatureAsync(featureKey, environmentKey, authKey);
    //
    //     // Assert
    //     ((Ok<ToggleFeatureResponse>)result).Value.Should().BeEquivalentTo(toggleFeatureResponse);
    // }
    //
    // [Fact]
    // public async Task ToggleFeatureAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.ToggleFeatureAsync(featureKey, environmentKey, authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task ToggleFeatureAsync_WhenNotFeatureAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.ToggleFeatureAsync(featureKey, environmentKey, authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task ToggleFeatureAsync_WhenNotEnvironmentAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.ToggleFeatureAsync(featureKey, environmentKey, authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task AddFeatureAsync_WhenCalled_ReturnsAddResponse()
    // {
    //     // Arrange
    //     const string featureName = "TestFeature";
    //     const string description = "TestDescription";
    //     var authKey = Guid.NewGuid();
    //     var addResponse = new AddResponse(featureName, Guid.NewGuid(), true, "Feature added");
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.AddFeatureAsync(featureName, description, authKey).Returns((addResponse.WasAdded, addResponse.Key, addResponse.Reason));
    //
    //     // Act
    //     var result = await _featureProvider.AddFeatureAsync(featureName, description, authKey);
    //
    //     // Assert
    //     ((Ok<AddResponse>)result).Value.Should().BeEquivalentTo(addResponse);
    // }
    //
    // [Fact]
    // public async Task AddFeatureAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     const string featureName = "TestFeature";
    //     const string description = "TestDescription";
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.AddFeatureAsync(featureName, description, authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentToFeatureAsync_WhenCalled_ReturnsOk()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(true);
    //     _repository.AddEnvironmentToFeatureAsync(featureKey, environmentKey).Returns((true, "Environment added"));
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((Ok)result).StatusCode.Should().Be(200);
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentToFeatureAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentToFeatureAsync_WhenNotFeatureAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentToFeatureAsync_WhenNotEnvironmentAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentToFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task DeleteFeatureAsync_WhenCalled_ReturnsOk()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.DeleteFeatureAsync(featureKey).Returns((true, "Feature deleted"));
    //
    //     // Act
    //     var result = await _featureProvider.DeleteFeatureAsync(featureKey);
    //
    //     // Assert
    //     ((Ok)result).StatusCode.Should().Be(200);
    // }
    //
    // [Fact]
    // public async Task DeleteFeatureAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.DeleteFeatureAsync(featureKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task DeleteFeatureAsync_WhenNotFeatureAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.DeleteFeatureAsync(featureKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task DeleteEnvironmentFromFeatureAsync_WhenCalled_ReturnsOk()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(true);
    //     _repository.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey).Returns((true, "Environment deleted"));
    //
    //     // Act
    //     var result = await _featureProvider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((Ok)result).StatusCode.Should().Be(200);
    // }
    //
    // [Fact]
    // public async Task DeleteEnvironmentFromFeatureAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task DeleteEnvironmentFromFeatureAsync_WhenFeatureAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task DeleteEnvironmentFromFeatureAsync_WhenNotEnvironmentAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var featureKey = Guid.NewGuid();
    //     var environmentKey = Guid.NewGuid();
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.IsFeatureAdminAsync(featureKey, authKey).Returns(true);
    //     _repository.IsEnvironmentAdminAsync(environmentKey, authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.DeleteEnvironmentFromFeatureAsync(featureKey, environmentKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentAsync_WhenCalled_ReturnsAddResponse()
    // {
    //     // Arrange
    //     const string environmentName = "TestEnvironment";
    //     var authKey = Guid.NewGuid();
    //     var addResponse = new AddResponse(environmentName, Guid.NewGuid(), true, "Environment added");
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.AddEnvironmentAsync(environmentName, authKey).Returns((addResponse.WasAdded, addResponse.Key, addResponse.Reason));
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentAsync(environmentName, authKey);
    //
    //     // Assert
    //     ((Ok<AddResponse>)result).Value.Should().BeEquivalentTo(addResponse);
    // }
    //
    // [Fact]
    // public async Task AddEnvironmentAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     const string environmentName = "TestEnvironment";
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.AddEnvironmentAsync(environmentName, authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task GetEnvironmentsAsync_WhenCalled_ReturnsListOfEnvironments()
    // {
    //     // Arrange
    //     var authKey = Guid.NewGuid();
    //     var environments = new List<Environment>
    //     {
    //         new(Guid.NewGuid(), "TestEnvironment1"),
    //         new(Guid.NewGuid(), "TestEnvironment2")
    //     };
    //
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.GetEnvironmentsAsync(authKey).Returns((true, environments, string.Empty));
    //
    //     // Act
    //     var result = await _featureProvider.GetEnvironmentsAsync(authKey);
    //
    //     // Assert
    //     ((Ok<List<Environment>>)result).Value.Should().BeEquivalentTo(environments);
    // }
    //
    // [Fact]
    // public async Task GetEnvironmentsAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.GetEnvironmentsAsync(authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
    //
    // [Fact]
    // public async Task GetFeaturesAsync_WhenCalled_ReturnsListOfFeatures()
    // {
    //     // Arrange
    //     var authKey = Guid.NewGuid();
    //     var features = new List<Feature>
    //     {
    //         new("TestFeature1", "TestDescription1", Guid.NewGuid(), new[] { new FeatureEnvironment(Guid.NewGuid(), "TestEnvironment1", true) }),
    //         new("TestFeature2", "TestDescription2", Guid.NewGuid(), new[] { new FeatureEnvironment(Guid.NewGuid(), "TestEnvironment1", true) }),
    //     };
    //
    //     _repository.IsAdminAsync(authKey).Returns(true);
    //     _repository.GetFeaturesAsync(authKey).Returns((true, features, string.Empty));
    //
    //     // Act
    //     var result = await _featureProvider.GetFeaturesAsync(authKey);
    //
    //     // Assert
    //     ((Ok<List<Feature>>)result).Value.Should().BeEquivalentTo(features);
    // }
    //
    // [Fact]
    // public async Task GetFeaturesAsync_WhenNotAdmin_ReturnsForbidden()
    // {
    //     // Arrange
    //     var authKey = Guid.NewGuid();
    //     _repository.IsAdminAsync(authKey).Returns(false);
    //
    //     // Act
    //     var result = await _featureProvider.GetFeaturesAsync(authKey);
    //
    //     // Assert
    //     ((ForbidHttpResult)result).AuthenticationSchemes.Should().BeEquivalentTo("Not authorized");
    // }
}
