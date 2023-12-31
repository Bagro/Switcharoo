using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Environments.DeleteEnvironment;
using Xunit;

namespace Switcharoo.Tests.Features.Environments.DeleteEnvironment;

public sealed class DeleteEnvironmentEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var deleteEnvironmentEndpoint = new DeleteEnvironmentEndpoint();
        
        // Act
        deleteEnvironmentEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapDelete("/environment/{environmentId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task DeleteEnvironmentEndpoint_SuccessfullyDeletesEnvironment_ReturnsOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentProvider = Substitute.For<IEnvironmentRepository>();

        environmentProvider.DeleteEnvironmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((true, string.Empty));

        // Act
        var result = await DeleteEnvironmentEndpoint.HandleAsync(Guid.NewGuid(), user, environmentProvider, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task DeleteEnvironmentEndpoint_UnSuccessfullyDeletesEnvironment_ReturnsBadRequest()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentProvider = Substitute.For<IEnvironmentRepository>();

        environmentProvider.DeleteEnvironmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((false, "Environment not found"));

        // Act
        var result = await DeleteEnvironmentEndpoint.HandleAsync(Guid.NewGuid(), user, environmentProvider, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
