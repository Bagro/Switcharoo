using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Switcharoo.Features.Environments;
using Switcharoo.Features.Environments.GetEnvironments;
using Xunit;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Tests.Features.Environments.GetEnvironments;

public sealed class GetEnvironmentsEndpointTests
{
    [Fact]
    public async Task HandleAsync_EnvironmentsFound_ShouldReturnOk()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        var environments = new List<Environment> { new() };
        environmentRepository.GetEnvironmentsAsync(Arg.Any<Guid>()).Returns(environments);

        // Act
        var result = await GetEnvironmentsEndpoint.HandleAsync(user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<List<Switcharoo.Features.Environments.Model.Environment>>>();
    }
    
    [Fact]
    public async Task HandleAsync_EnvironmentsFound_ShouldReturnEnvironments()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        var environments = new List<Environment> { 
            new() { Id = Guid.NewGuid(), Name = "Test Environment"}, 
            new() { Id = Guid.NewGuid(), Name = "Test Environment 2"} };
        
        environmentRepository.GetEnvironmentsAsync(Arg.Any<Guid>()).Returns(environments);
        
        var expectedEnvironments = environments.Select(
                environment => new Switcharoo.Features.Environments.Model.Environment
                {
                    Id = environment.Id,
                    Name = environment.Name
                })
            .ToList();

        // Act
        var result = await GetEnvironmentsEndpoint.HandleAsync(user, environmentRepository, CancellationToken.None);

        // Assert
        var okValue = ((Ok<List<Switcharoo.Features.Environments.Model.Environment>>) result).Value;
        okValue.Should().BeEquivalentTo(expectedEnvironments);
    }
    
    [Fact]
    public async Task HandleAsync_NoEnvironmentsFound_ShouldReturnNotFound()
    {
        // Arrange
        var user = UserHelper.GetClaimsPrincipalWithClaims();
        var environmentRepository = Substitute.For<IEnvironmentRepository>();
        environmentRepository.GetEnvironmentsAsync(Arg.Any<Guid>()).Returns(new List<Environment>());

        // Act
        var result = await GetEnvironmentsEndpoint.HandleAsync(user, environmentRepository, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFound<string>>();
    }
}
