using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Environments.AddEnvironment;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Environments.AddEnvironment;

public sealed class AddEnvironmentEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var addEnvironmentEndpoint = new AddEnvironmentEndpoint();
        
        // Act
        addEnvironmentEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPost("/environment", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_SuccessfullyAddNewEnvironment_ReturnsOk()
    {
        // Arrange
        var request = new AddEnvironmentRequest("Test Environment");
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        
        environmentRepository.IsNameAvailableAsync(request.Name, user.GetUserId()).Returns(true);
        environmentRepository.GetUserAsync(user.GetUserId()).Returns(UserFakes.GetFakeUser());

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task HandleAsync_NameExists_ReturnsConflict()
    {
        // Arrange
        var request = new AddEnvironmentRequest("Test Environment");
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        
        environmentRepository.IsNameAvailableAsync(request.Name, user.GetUserId()).Returns(false);
        environmentRepository.GetUserAsync(user.GetUserId()).Returns(UserFakes.GetFakeUser());

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddEnvironmentRequest("Test Environment");
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        
        environmentRepository.IsNameAvailableAsync(request.Name, user.GetUserId()).Returns(true);
        environmentRepository.GetUserAsync(user.GetUserId()).Returns((User?)null);

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
