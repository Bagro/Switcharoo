using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
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
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var getEnvironmentEndpoint = new GetEnvironmentEndpoint();
        
        // Act
        getEnvironmentEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapGet("/environment/{environmentId}", dummyRequestDelegate).RequireAuthorization();
    }
    
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
