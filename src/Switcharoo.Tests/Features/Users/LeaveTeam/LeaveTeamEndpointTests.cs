using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Users;
using Switcharoo.Features.Users.LeaveTeam;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Users.LeaveTeam;

public sealed class LeaveTeamEndpointTests
{
    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);

        var leaveTeamEndpoint = new LeaveTeamEndpoint();

        // Act
        var result = await leaveTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User does not exist");
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAMemberOfATeam_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User());

        var leaveTeamEndpoint = new LeaveTeamEndpoint();

        // Act
        var result = await leaveTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User is not a member of a team");
    }

    [Fact]
    public async Task HandleAsync_WhenTeamDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Team = TeamFakes.GetFakeTeam();

        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns((Team?)null);

        var leaveTeamEndpoint = new LeaveTeamEndpoint();

        // Act
        var result = await leaveTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team does not exist");
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAMemberOfTheTeam_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        fakeUser.Team = TeamFakes.GetFakeTeam();
        var fakeTeam = TeamFakes.GetFakeTeam();

        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);

        var leaveTeamEndpoint = new LeaveTeamEndpoint();

        // Act
        var result = await leaveTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User is not a member of this team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAMemberOfTheTeam_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        fakeUser.Team = TeamFakes.GetFakeTeam();
        var fakeTeam = TeamFakes.GetFakeTeam();
        fakeTeam.Members.Add(fakeUser);

        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);

        var leaveTeamEndpoint = new LeaveTeamEndpoint();

        // Act
        var result = await leaveTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
}
