using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Users;
using Switcharoo.Features.Users.AcceptTeamInvite;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Users.AcceptTeamInvite;

public sealed class AcceptTeamInviteEndpointTests
{
    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyAMemberOfATeam_ReturnsConflict()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User { Team = new Team() });
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("User is already a member of a team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamInviteDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User());
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns((TeamInvite?)null);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team invite not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamInviteHasExpired_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User());
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns(new TeamInvite { ExpiresAt = DateTime.UtcNow.AddDays(-1) });
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team invite has expired");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamInviteHasBeenUsed_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();
        teamInvite.ActivatedByUser = UserFakes.GetFakeUser();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User());
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns(teamInvite);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team invite has already been used");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(new User());
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns(teamInvite);
        userRepository.GetTeamAsync(Arg.Any<Guid>()).Returns((Team?)null);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyAMemberOfTheTeam_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();
        var team = TeamFakes.GetFakeTeam();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        team.Members.Add(fakeUser);
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns(teamInvite);
        userRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User is already a member of this team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenEverythingIsOk_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();
        var team = TeamFakes.GetFakeTeam();
        var fakeUser = UserFakes.GetFakeUser();
        fakeUser.Id = user.GetUserId();
        
        var userRepository = Substitute.For<IUserFeatureRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(fakeUser);
        userRepository.GetTeamInviteAsync(Arg.Any<Guid>()).Returns(teamInvite);
        userRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);
        
        var endpoint = new AcceptTeamInviteEndpoint();
        
        // Act
        var result = await endpoint.HandleAsync(Guid.NewGuid(), user, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok>();
    }
}
