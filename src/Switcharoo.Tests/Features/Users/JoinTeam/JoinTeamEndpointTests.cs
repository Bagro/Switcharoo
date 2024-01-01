using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Users;
using Switcharoo.Features.Users.JoinTeam;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Users.JoinTeam;

public sealed class JoinTeamEndpointTests
{
    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);

        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User does not exist");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyAMemberOfATeam_ReturnsConflict()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Team = TeamFakes.GetFakeTeam();
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        
        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("User is already a member of a team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns((Team?)null);
        
        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team does not exist");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamIsInviteOnly_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        var fakeTeam = TeamFakes.GetFakeTeam();
        fakeTeam.InviteOnly = true;
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);
        
        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team is invite only");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyAMemberOfTheTeam_ReturnsConflict()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        
        var fakeTeam = TeamFakes.GetFakeTeam();
        fakeTeam.Members.Add(fakeUser);
        fakeTeam.InviteOnly = false;
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);
        
        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("You are already a member of this team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsNotAMemberOfTheTeam_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        
        var fakeTeam = TeamFakes.GetFakeTeam();
        fakeTeam.InviteOnly = false;
        
        var userFeatureRepository = Substitute.For<IUserFeatureRepository>();
        userFeatureRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userFeatureRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(fakeTeam);
        
        var joinTeamEndpoint = new JoinTeamEndpoint();

        // Act
        var result = await joinTeamEndpoint.HandleAsync(Guid.NewGuid(), user, userFeatureRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
}
