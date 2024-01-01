using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Extensions;
using Switcharoo.Features.Teams;
using Switcharoo.Features.Teams.DeleteTeam;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.DeleteTeam;

public sealed class DeleteTeamEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var deleteTeamEndpoint = new DeleteTeamEndpoint();
        
        // Act
        deleteTeamEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapDelete("/team/{teamId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamNotFound_ReturnsBadRequest()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        // Act
        var result = await new DeleteTeamEndpoint().HandleAsync(Guid.NewGuid(), user, teamRepository);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("Team not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenUserNotTeamOwner_ReturnsBadRequest()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();

        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(TeamFakes.GetFakeTeam());
        
        // Act
        var result = await new DeleteTeamEndpoint().HandleAsync(Guid.NewGuid(), user, teamRepository);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("You don't have permission to delete this team");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamIsDeleted_ReturnsOk()
    {
        // Arrange
        var teamRepository = Substitute.For<ITeamRepository>();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var team = TeamFakes.GetFakeTeam();
        team.Owner.Id = user.GetUserId();

        teamRepository.GetTeamAsync(Arg.Any<Guid>()).Returns(team);
        
        // Act
        var result = await new DeleteTeamEndpoint().HandleAsync(team.Id, user, teamRepository);
        
        // Assert
        result.Should().BeOfType<Ok>();
    }
}
