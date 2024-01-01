using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Common;
using Switcharoo.Features.Teams.AddTeam;
using Switcharoo.Interfaces;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.AddTeam;

public sealed class AddTeamEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var addTeamEndpoint = new AddTeamEndpoint();
        
        // Act
        addTeamEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPost("/team", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_ValidInput_ReturnsOk()
    {
        // Arrange
        var request = new AddTeamRequest("Test Team", "Test Description", true, false);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>()).Returns(true);
        
        var addTeamEndpoint = new AddTeamEndpoint();

        // Act
        var result = await addTeamEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Ok<AddTeamResponse>>();
    }

    [Fact]
    public async Task HandleAsync_ValidInput_CallsRepositoryWithCorrectValues()
    {
        // Arrange
        var request = new AddTeamRequest("Test Team", "Test Description", true, false);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();
    
        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>()).Returns(true);
        
        var addTeamEndpoint = new AddTeamEndpoint();
        
        Database.Entities.Team team = null;
        await teamRepository.AddTeamAsync(Arg.Do<Database.Entities.Team>( x => team = x));

        // Act
        await addTeamEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);
        
        // Assert
       Compare(team, request).Should().BeTrue();
    }
    
    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddTeamRequest("Test Team", "Test Description", true, false);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns((Database.Entities.User)null);
        teamRepository.IsNameAvailableAsync(Arg.Any<string>()).Returns(true);
        
        var addTeamEndpoint = new AddTeamEndpoint();
        
        // Act
        var result = await addTeamEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Fact]
    public async Task HandleAsync_NameNotAvailable_ReturnsConflict()
    {
        // Arrange
        var request = new AddTeamRequest("Test Team", "Test Description", true, false);

        var user = UserHelper.GetClaimsPrincipalWithClaims();
        
        var teamRepository = Substitute.For<ITeamRepository>();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository.GetUserAsync(Arg.Any<Guid>()).Returns(UserFakes.GetFakeUser());
        teamRepository.IsNameAvailableAsync(Arg.Any<string>()).Returns(false);
        
        var addTeamEndpoint = new AddTeamEndpoint();
        
        // Act
        var result = await addTeamEndpoint.HandleAsync(request, user, teamRepository, userRepository, CancellationToken.None);
        
        // Assert
        result.Should().BeOfType<Conflict<string>>();
    }

    private bool Compare(Database.Entities.Team team, AddTeamRequest request)
    {
        return team.Name == request.Name
               && team.Description == request.Description
               && team.AllCanManage == request.AllCanManage
               && team.InviteOnly == request.InviteOnly;
    }
}
