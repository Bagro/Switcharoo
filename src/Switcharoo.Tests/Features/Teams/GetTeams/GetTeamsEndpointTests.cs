using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Features.Teams.GetTeams;
using Switcharoo.Features.Teams.Model;
using Switcharoo.Interfaces;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.GetTeams;

public sealed class GetTeamsEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var getTeamsEndpoint = new GetTeamsEndpoint();
        // Act
        getTeamsEndpoint.MapEndpoint(endpoints);

        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapGet("/teams", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamRepositoryReturnsNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        teamRepository.GetTeamsAsync(Arg.Any<Guid>()).Returns((false, [], "Not found"));
        
        // Act
        var result = await GetTeamsEndpoint.HandleAsync(user, teamRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<NotFound<string>>().Which.Value.Should().Be("Not found");
    }
    
    [Fact]
    public async Task HandleAsync_WhenTeamRepositoryReturnsTeams_ShouldReturnOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var teamRepository = Substitute.For<ITeamRepository>();
        teamRepository.GetTeamsAsync(Arg.Any<Guid>()).Returns((true, [TeamFakes.GetFakeTeam()], ""));
        
        // Act
        var result = await GetTeamsEndpoint.HandleAsync(user, teamRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok<List<Team>>>();
    }
}
