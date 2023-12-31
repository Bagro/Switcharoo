using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams.GetTeam;
using Switcharoo.Interfaces;
using Switcharoo.Tests.Common;
using Xunit;
using Team = Switcharoo.Features.Teams.Model.Team;

namespace Switcharoo.Tests.Features.Teams.GetTeam;

public sealed class GetTeamEndpointTests
{
    [Fact]
    public async Task HandleAsync_WhenTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        // Act
        var result = await GetTeamEndpoint.HandleAsync(Guid.NewGuid(), user, teamRepository);
        
        // Assert
        result.Should().BeOfType<NotFound<string>>().Which.Value.Should().Be("Team not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserNotTeamOwnerAndTeamNotPublic_ReturnsForbidden()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var team = TeamFakes.GetFakeTeam();
        team.AllCanManage = false;
        team.Members = new List<User>();
        
        teamRepository.GetTeamAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(team);
        
        // Act
        var result = await GetTeamEndpoint.HandleAsync(Guid.NewGuid(), user, teamRepository);
        
        // Assert
        result.Should().BeOfType<ForbidHttpResult>();
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserNotTeamOwnerAndTeamPublic_ReturnsOk()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var team = TeamFakes.GetFakeTeam();
        team.AllCanManage = true;
        team.Members = new List<User>();
        
        teamRepository.GetTeamAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(team);
        
        // Act
        var result = await GetTeamEndpoint.HandleAsync(Guid.NewGuid(), user, teamRepository);
        
        // Assert
        result.Should().BeOfType<Ok<Team>>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTeamOwner_ReturnsOk()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        var team = TeamFakes.GetFakeTeam();
        team.Owner.Id = user.GetUserId();

        teamRepository.GetTeamAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(team);

        // Act
        var result = await GetTeamEndpoint.HandleAsync(Guid.NewGuid(), user, teamRepository);

        // Assert
        result.Should().BeOfType<Ok<Team>>();
    }
}
