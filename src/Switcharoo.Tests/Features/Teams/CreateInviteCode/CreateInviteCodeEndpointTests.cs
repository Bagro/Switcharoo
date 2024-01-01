using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams.CreateInviteCode;
using Switcharoo.Interfaces;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.CreateInviteCode;

public sealed class CreateInviteCodeEndpointTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateInviteCodeRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns((User?)null);

        // Act
        var result = await CreateInviteCodeEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("User not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateInviteCodeRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var storedUser = UserFakes.GetFakeUser();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(storedUser);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns((Team?)null);

        // Act
        var result = await CreateInviteCodeEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserIsNotOwner_ReturnsForbid()
    {
        // Arrange
        var request = new CreateInviteCodeRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var storedUser = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(storedUser);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);

        // Act
        var result = await CreateInviteCodeEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidHttpResult>();
    }
    
    [Fact]
    public async Task HandleAsync_WhenInviteCodeCreated_ReturnsOk()
    {
        // Arrange
        var request = new CreateInviteCodeRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var storedUser = UserFakes.GetFakeUser();
        storedUser.Id = user.GetUserId();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = storedUser;
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(storedUser);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);

        // Act
        var result = await CreateInviteCodeEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<CreateInviteCodeResponse>>();
    }
    
    [Fact]
    public async Task HandleAsync_WhenInviteCodeCreated_ReturnsInviteCode()
    {
        // Arrange
        var request = new CreateInviteCodeRequest(Guid.NewGuid());
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var storedUser = UserFakes.GetFakeUser();
        storedUser.Id = user.GetUserId();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = storedUser;
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(storedUser);
        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);

        // Act
        var result = await CreateInviteCodeEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<CreateInviteCodeResponse>>().Which.Value.InviteCode.Should().NotBe(Guid.Empty);
    }
}
