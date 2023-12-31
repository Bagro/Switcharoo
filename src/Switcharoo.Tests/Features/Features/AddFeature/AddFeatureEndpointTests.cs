using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.AddFeature;
using Switcharoo.Features.Features.Model;
using Switcharoo.Tests.Common;
using Xunit;
using Feature = Switcharoo.Database.Entities.Feature;

namespace Switcharoo.Tests.Features.Features.AddFeature;

public sealed class AddFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var addFeatureEndpoint = new AddFeatureEndpoint();
        
        // Act
        addFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPost("/feature", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);

        var environment = EnvironmentFakes.GetFakeEnvironment();
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(environment);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", 
        [
            new FeatureUpdateEnvironment(environment.Id, true),
        ]);

        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok<AddFeatureResponse>>();
    }
    
    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldReturnIdForFeature()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);

        var environment = EnvironmentFakes.GetFakeEnvironment();
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(environment);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", 
        [
            new FeatureUpdateEnvironment(environment.Id, true),
        ]);

        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        ((Ok<AddFeatureResponse>) result).Value.Id.Should().NotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldAddFeature()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);

        var environment = EnvironmentFakes.GetFakeEnvironment();
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(environment);
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var userRepository = Substitute.For<IUserRepository>();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", 
        [
            new FeatureUpdateEnvironment(environment.Id, true),
        ]);
        
        Feature feature = null;
        await featureRepository.AddFeatureAsync(Arg.Do<Feature>(x => feature = x));
        
        // Act
        await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        feature.Should().NotBeNull("feature should have been added");
        feature.Id.Should().NotBe(Guid.Empty, "feature should hanve an id");
        feature.Name.Should().Be(addFeatureRequest.Name, "feature should have the correct name");
        feature.Key.Should().Be(addFeatureRequest.Key, "feature should have the correct key");
        feature.Description.Should().Be(addFeatureRequest.Description, "feature should have the correct description");
        feature.Environments.Should().HaveCount(1);
        feature.Environments[0].Environment.Should().BeEquivalentTo(environment, "feature should have the correct environment");
        feature.Environments[0].IsEnabled.Should().BeTrue();
        feature.Owner.Should().BeEquivalentTo(fakeUser, "feature should have the correct owner");
    }
    
    [Fact]
    public async Task HandleAsync_FeatureNameInUse_ShouldReturnConflictWithCorrectReason()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(false);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", null);
        
        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("Name is already in use");
    }
    
    [Fact]
    public async Task HandleAsync_FeatureKeyInUse_ShouldReturnConflictWithCorrectReason()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(false);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", null);
        
        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("Key is already in use");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_NameIsNullOrEmpty_ShouldReturnReturnBadRequestWithCorrectReason(string? name)
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest(name, "test-feature", "Test feature", null);
        
        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Name is required");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_KeyIsNullOrEmpty_ShouldReturnReturnBadRequestWithCorrectReason(string? key)
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", key, "Test feature", null);
        
        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Key is required");
    }
    
    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnBadRequestWithCorrectReason()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", null);
        
        // Act
        var result = await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User not found");
    }
    
    [Fact]
    public async Task HandleAsync_ValidRequest_ShouldCallRepository()
    {
        // Arrange
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var addFeatureRequest = new AddFeatureRequest("Test Feature", "test-feature", "Test feature", null);
        
        // Act
        await AddFeatureEndpoint.HandleAsync(addFeatureRequest, user , featureRepository, userRepository, CancellationToken.None);
        
        // Assert
        await featureRepository.Received(1).AddFeatureAsync(Arg.Any<Feature>());
    }
}
