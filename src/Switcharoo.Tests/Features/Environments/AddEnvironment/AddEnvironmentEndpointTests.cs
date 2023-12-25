using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Common;
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
    public async Task HandleAsync_SuccessfullyAddNewEnvironment_ReturnsOk()
    {
        // Arrange
        var request = new AddEnvironmentRequest("Test Environment");
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.IsNameAvailableAsync(request.Name, user.GetUserId()).Returns(true);
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(user.GetUserId()).Returns(UserFakes.GetFakeUser());

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, userRepository, CancellationToken.None);

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
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(user.GetUserId()).Returns(UserFakes.GetFakeUser());

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, userRepository, CancellationToken.None);

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
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserAsync(user.GetUserId()).Returns((User?)null);

        // Act
        var result = await AddEnvironmentEndpoint.HandleAsync(request, user, environmentRepository, userRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
