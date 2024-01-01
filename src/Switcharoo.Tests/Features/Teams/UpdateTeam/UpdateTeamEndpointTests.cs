using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams;
using Switcharoo.Features.Teams.UpdateTeam;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.UpdateTeam;

public sealed class UpdateTeamEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var updateTeamEndpoint = new UpdateTeamEndpoint();
        // Act
        updateTeamEndpoint.MapEndpoint(endpoints);

        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPut("/team", dummyRequestDelegate).RequireAuthorization();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnBadRequestWithReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();

        teamRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);

        var updateTeamRequest = new UpdateTeamRequest(Guid.NewGuid(), "name", "description", false, false, [], []);

        // Act
        var result = await UpdateTeamEndpoint.HandleAsync(updateTeamRequest, user, teamRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task HandleAsync_NameNotAvailable_ShouldReturnConflictWithReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();

        teamRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(false);

        var updateTeamRequest = new UpdateTeamRequest(Guid.NewGuid(), "name", "description", false, false, [], []);

        // Act
        var result = await UpdateTeamEndpoint.HandleAsync(updateTeamRequest, user, teamRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("Name is already in use");
    }

    [Fact]
    public async Task HandleAsync_TeamNotFound_ShouldReturnBadRequestWithReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();

        teamRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns((Team?)null);

        var updateTeamRequest = new UpdateTeamRequest(Guid.NewGuid(), "name", "description", false, false, [], []);

        // Act
        var result = await UpdateTeamEndpoint.HandleAsync(updateTeamRequest, user, teamRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team not found");
    }

    [Fact]
    public async Task HandleAsync_UserNotAllowedToUpdateTeam_ShouldReturnForbid()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();

        teamRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(TeamFakes.GetFakeTeam());

        var updateTeamRequest = new UpdateTeamRequest(Guid.NewGuid(), "name", "description", false, false, [], []);

        // Act
        var result = await UpdateTeamEndpoint.HandleAsync(updateTeamRequest, user, teamRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidHttpResult>();
    }

    [Fact]
    public async Task HandleAsync_TeamUpdated_ShouldReturnOkWithReason()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();

        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();

        var fakeTeam = TeamFakes.GetFakeTeam();
        fakeTeam.Owner = fakeUser;

        var fakeEnvironments = EnvironmentFakes.GetFakeEnvironments();
        var index = 0;
        foreach (var fakeEnvironment in fakeEnvironments)
        {
            if (index >= fakeTeam.Environments.Count)
            {
                break;
            }

            fakeTeam.Environments[index].Environment = fakeEnvironment;
            index++;
        }

        List<Feature> fakeFeatures = [FeatureFakes.GetFakeFeature(), FeatureFakes.GetFakeFeature()];
        index = 0;
        foreach (var fakeFeature in fakeFeatures)
        {
            if (index >= fakeTeam.Features.Count)
            {
                break;
            }

            fakeTeam.Features[index].Feature = fakeFeature;
            index++;
        }
        
        teamRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        teamRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(true);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);

        teamRepository.GetEnvironmentsForIdAsync(Arg.Any<List<Guid>>(), Arg.Any<Guid>()).Returns(fakeEnvironments);
        teamRepository.GetFeaturesForIdAsync(Arg.Any<List<Guid>>(), Arg.Any<Guid>()).Returns(fakeFeatures);

        var updateTeamRequest = new UpdateTeamRequest(Guid.NewGuid(), "name", "description", false, false, [], []);

        foreach (var fakeEnvironment in fakeEnvironments)
        {
            updateTeamRequest.Environments.Add(new UpdateTeamEnvironment(fakeEnvironment.Id, true));
        }
        
        foreach (var fakeFeature in fakeFeatures)
        {
            updateTeamRequest.Features.Add(new UpdateTeamFeature(fakeFeature.Id, true, true));
        }
        
        // Act
        var result = await UpdateTeamEndpoint.HandleAsync(updateTeamRequest, user, teamRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<string>>().Which.Value.Should().Be("Team updated");
    }
}
