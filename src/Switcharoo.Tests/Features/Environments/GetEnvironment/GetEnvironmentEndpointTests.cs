using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Extensions;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Environments.GetEnvironment;
using Xunit;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Tests.Features.Environments.GetEnvironment;

public sealed class GetEnvironmentEndpointTests
{
    [Fact]
    public async Task HandleAsync_EnvironmentExists_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.GetEnvironmentAsync(id, user.GetUserId()).Returns(new Environment { Id = id, Name = "Test Environment" });

        // Act
        var result = await GetEnvironmentEndpoint.HandleAsync(id, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<Switcharoo.Features.Environments.Model.Environment>>();
    }

    [Fact]
    public async Task HandleAsync_EnvironmentExists_ReturnsEnvironment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.GetEnvironmentAsync(id, user.GetUserId()).Returns(new Environment { Id = id, Name = "Test Environment" });
        
        var expectedEnvironment = new Switcharoo.Features.Environments.Model.Environment
        {
            Id = id,
            Name = "Test Environment",
        };

        // Act
        var result = await GetEnvironmentEndpoint.HandleAsync(id, user, environmentRepository, CancellationToken.None);

        // Assert
        var okValue = ((Ok<Switcharoo.Features.Environments.Model.Environment>) result).Value;
        okValue.Should().BeEquivalentTo(expectedEnvironment);
    }

    [Fact]
    public async Task HandleAsync_EnvironmentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.GetEnvironmentAsync(id, user.GetUserId()).Returns((Environment)null);

        // Act
        var result = await GetEnvironmentEndpoint.HandleAsync(id, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFound<string>>();
    }
}
