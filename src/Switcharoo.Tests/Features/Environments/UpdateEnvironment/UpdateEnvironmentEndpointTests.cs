using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Switcharoo.Extensions;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Environments.UpdateEnvironment;
using Xunit;
using Environment = Switcharoo.Features.Environments.Model.Environment;

namespace Switcharoo.Tests.Features.Environments.UpdateEnvironment;

public sealed class UpdateEnvironmentEndpointTests
{
    [Fact]
    public void MapEndpoint_ShouldMapEndpointAndRequireAuthorization()
    {
        // Arrange
        var endpoints = Substitute.For<IEndpointRouteBuilder>();
        var updateEnvironmentEndpoint = new UpdateEnvironmentEndpoint();
        
        // Act
        updateEnvironmentEndpoint.MapEndpoint(endpoints);
        
        // Assert
        var dummyRequestDelegate = Substitute.For<RequestDelegate>();
        endpoints.Received().MapPut("/environment/{environmentId}", dummyRequestDelegate).RequireAuthorization();
    }
    
    [Fact]
    public async Task HandleAsync_UpdateExistingEnvironment_ReturnsOk()
    {
        // Arrange
        var environment = new Environment { Id = Guid.NewGuid(), Name = "Test Environment" };
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.IsNameAvailableAsync(environment.Name, environment.Id, user.GetUserId()).Returns(true);
        environmentRepository.GetEnvironmentAsync(environment.Id, user.GetUserId()).Returns(new Switcharoo.Database.Entities.Environment { Id = environment.Id, Name = environment.Name });
   
        // Act
        var result = await  UpdateEnvironmentEndpoint.HandleAsync(environment, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public async Task HandleAsync_EnvironmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var environment = new Environment { Id = Guid.NewGuid(), Name = "Test Environment" };
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.GetEnvironmentAsync(environment.Id, user.GetUserId()).Returns((Switcharoo.Database.Entities.Environment)null);
        environmentRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

        // Act
        var result = await UpdateEnvironmentEndpoint.HandleAsync(environment, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFound<string>>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_EmptyNames_ReturnsBadRequest(string name)
    {
        // Arrange
        var environment = new Environment { Id = Guid.NewGuid(), Name = name };
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.IsNameAvailableAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(true));

        // Act
        var result = await UpdateEnvironmentEndpoint.HandleAsync(environment, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Fact]
    public async Task UpdateEnvironment_NameAlreadyInUse_ReturnsConflict()
    {
        // Arrange
        var environment = new Environment { Id = Guid.NewGuid(), Name = "Test Environment" };
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.IsNameAvailableAsync(environment.Name, environment.Id, user.GetUserId()).Returns(false);
        environmentRepository.GetEnvironmentAsync(environment.Id, user.GetUserId()).Returns(new Switcharoo.Database.Entities.Environment { Id = environment.Id, Name = environment.Name });

        // Act
        var result = await UpdateEnvironmentEndpoint.HandleAsync(environment, user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Conflict<string>>();
    }
    
    

}
