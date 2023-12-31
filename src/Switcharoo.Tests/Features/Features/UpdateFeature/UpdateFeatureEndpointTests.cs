using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Extensions;
using Switcharoo.Features.Features;
using Switcharoo.Features.Features.Model;
using Switcharoo.Features.Features.UpdateFeature;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Features.UpdateFeature;

public sealed class UpdateFeatureEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var updateFeatureEndpoint = new UpdateFeatureEndpoint();
        
        // Act
        updateFeatureEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPut("/feature", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_ValidInput_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        var feature = FeatureFakes.GetFakeFeature();

        SetFeatureIds(feature, updateFeatureRequest);

        var featureRepository = Substitute.For<IFeatureRepository>();

        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.IsKeyAvailableAsync(updateFeatureRequest.Key, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.UpdateFeatureAsync(Arg.Any<Switcharoo.Database.Entities.Feature>()).Returns(Task.CompletedTask);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task HandleAsync_FeatureNoteFound_ReturnsNotFound()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        
        var featureRepository = Substitute.For<IFeatureRepository>();

        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns((Switcharoo.Database.Entities.Feature?) null);
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        
        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task HandleAsync_NewNameAndKey_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), "New Feature", "new-feature");

        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);

        var featureRepository = Substitute.For<IFeatureRepository>();
        
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsKeyAvailableAsync(updateFeatureRequest.Key, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.UpdateFeatureAsync(Arg.Any<Switcharoo.Database.Entities.Feature>()).Returns(Task.CompletedTask);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task HandleAsync_NewNameAndKey_ShouldUpdateNameAndKey()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), "New Feature", "new-feature");

        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);

        var featureRepository = Substitute.For<IFeatureRepository>();
        
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsKeyAvailableAsync(updateFeatureRequest.Key, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.UpdateFeatureAsync(Arg.Any<Switcharoo.Database.Entities.Feature>()).Returns(Task.CompletedTask);

        var nameAndKeySetToNew = false;
        await featureRepository.UpdateFeatureAsync(Arg.Do<Switcharoo.Database.Entities.Feature>(x => nameAndKeySetToNew = x.Name == updateFeatureRequest.Name && x.Key == updateFeatureRequest.Key));
        
        // Act
        await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        nameAndKeySetToNew.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_NullOrEmptyName_ReturnsBadRequest(string? name)
    {
        // Arrange
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), name);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_NullOrEmptyKey_ReturnsBadRequest(string? key)
    {
        // Arrange
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), key: key);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsBadRequest()
    {
        // Arrange
        var updateFeatureRequest = new UpdateFeatureRequest(Guid.Empty, "New Feature", "new-feature", "Updated feature description", [new FeatureUpdateEnvironment(Guid.NewGuid(), true)]);
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Fact]
    public async Task HandleAsync_UpdateFeatureWithNewEnvironment_ReturnsOk()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), environmentCount: 2);
        
        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Id = environmentId;
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), user.GetUserId()).Returns(EnvironmentFakes.GetFakeEnvironment());
        featureRepository.GetEnvironmentAsync(environmentId, user.GetUserId()).Returns(environment);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task HandleAsync_UpdateFeatureWithNewEnvironment_ShouldAddEnvironment()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid(), environmentCount: 2);
        
        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);
        
        var addedEnvironmentId = updateFeatureRequest.Environments[1].Id;
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Id = addedEnvironmentId;
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), user.GetUserId()).Returns(EnvironmentFakes.GetFakeEnvironment());
        featureRepository.GetEnvironmentAsync(addedEnvironmentId, user.GetUserId()).Returns(environment);

        var environmentAdded = false;
        await featureRepository.UpdateFeatureAsync(Arg.Do<Switcharoo.Database.Entities.Feature>(x => environmentAdded = x.Environments.Exists(y => y.Environment.Id == addedEnvironmentId)));
        
        // Act
        await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        environmentAdded.Should().BeTrue();
    }
    
    [Fact]
    public async Task HandleAsync_UpdatesFeatureWithRemovedEnvironment_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
 
        var feature = FeatureFakes.GetFakeFeature(2);
        SetFeatureIds(feature, updateFeatureRequest);
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task HandleAsync_UpdatesFeatureWithRemovedEnvironment_ShouldRemoveEnvironment()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
 
        var feature = FeatureFakes.GetFakeFeature(2);
        SetFeatureIds(feature, updateFeatureRequest);

        var removeEnvironementId = feature.Environments[1].Environment.Id;
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);

        var environmentRemoved = false;
        await featureRepository.UpdateFeatureAsync(Arg.Do<Switcharoo.Database.Entities.Feature>(x => environmentRemoved = x.Environments.TrueForAll(y => y.Environment.Id != removeEnvironementId)));

        // Act
        await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        environmentRemoved.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_UpdateFeatureWithModifiedEnvironment_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        
        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);
        
        feature.Environments[0].IsEnabled = !updateFeatureRequest.Environments[0].IsEnabled;
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task HandleAsync_UpdateFeatureWithModifiedEnvironment_ShouldSendUpdatedEnvironment()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        
        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);
        
        feature.Environments[0].IsEnabled = !updateFeatureRequest.Environments[0].IsEnabled;
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.IsKeyAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Id = updateFeatureRequest.Environments[0].Id;
        featureRepository.GetEnvironmentAsync(Arg.Any<Guid>(), user.GetUserId()).Returns(environment);

        var resultingEnvironmentIsEnabled = false;
        await featureRepository.UpdateFeatureAsync(Arg.Do<Switcharoo.Database.Entities.Feature>(x => resultingEnvironmentIsEnabled = x.Environments[0].IsEnabled));
        
        // Act
        await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        resultingEnvironmentIsEnabled.Should().Be(updateFeatureRequest.Environments[0].IsEnabled);
    }
    
    [Fact]
    public async Task HandleAsync_NameAlreadyInUse_ReturnsConflict()
    {
        // Arrange
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var featureRepository = Substitute.For<IFeatureRepository>();
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(false);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>();
    }
    
    [Fact]
    public async Task HandleAsync_KeyIsAlreadyInUse_ReturnsConflict()
    {
        // Arrange
        var updateFeatureRequest = GetUpdateFeatureRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var feature = FeatureFakes.GetFakeFeature();
        SetFeatureIds(feature, updateFeatureRequest);
        
        var featureRepository = Substitute.For<IFeatureRepository>();
        
        featureRepository.IsNameAvailableAsync(updateFeatureRequest.Name, updateFeatureRequest.Id, user.GetUserId()).Returns(true);
        featureRepository.GetFeatureAsync(updateFeatureRequest.Id, user.GetUserId()).Returns(feature);
        featureRepository.IsKeyAvailableAsync(updateFeatureRequest.Key, updateFeatureRequest.Id, user.GetUserId()).Returns(false);

        // Act
        var result = await UpdateFeatureEndpoint.HandleAsync(updateFeatureRequest, user, featureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>();
    }

    private static UpdateFeatureRequest GetUpdateFeatureRequest(Guid id, string? name = "Test Feature", string key = "test-key", string description = "Test Description", int environmentCount = 1)
    {
        var updateFeatureRequest = new UpdateFeatureRequest(id, name, key, description, []);

        for (var i = 0; i < environmentCount; i++)
        {
            updateFeatureRequest.Environments.Add(new FeatureUpdateEnvironment(Guid.NewGuid(), true));
        }

        return updateFeatureRequest;
    }

    private static void SetFeatureIds(Switcharoo.Database.Entities.Feature feature, UpdateFeatureRequest updateFeatureRequest)
    {
        feature.Id = updateFeatureRequest.Id;

        if (feature.Environments.Count == 0 || updateFeatureRequest.Environments.Count == 0)
        {
            return;
        }

        var index = 0;
        foreach (var environment in feature.Environments)
        {
            environment.Environment.Id = updateFeatureRequest.Environments[index].Id;
            index++;

            if (index >= updateFeatureRequest.Environments.Count)
            {
                break;
            }
        }
    }
}
