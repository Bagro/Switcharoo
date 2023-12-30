using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Switcharoo.Features.Environments;
using Xunit;
using Environment = Switcharoo.Database.Entities.Environment;

namespace Switcharoo.Tests.Features.Environments;

public sealed class EnvironmentRepositoryTests
{
    private readonly DbContextOptions<SqliteDbContext> _dbContextOptions = new DbContextOptionsBuilder<SqliteDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDbForTesting")
        .EnableSensitiveDataLogging()
        .Options;

    [Fact]
    public async Task AddEnvironmentAsync_NewEnvironmentAdded_ContextShouldContainEnvironment()
    {
        // Arrange
        var environment = new Environment { Id = Guid.NewGuid(), Name = "Test Environment" };
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new EnvironmentRepository(context);

        // Act
        await repository.AddEnvironmentAsync(environment);

        // Assert
        (await context.Environments.SingleAsync(x => x.Id == environment.Id)).Should().BeEquivalentTo(environment);
    }

    [Fact]
    public async Task GetEnvironmentsAsync_EnvironmentsFound_ReturnsListOfEnvironmentsOwnedByUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environments = new List<Environment>
        {
            new()
                { Id = Guid.NewGuid(), Name = "Environment 1", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 2", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 3", Owner = new User { Id = Guid.NewGuid() } },
        };
        
        var expectedEnvironments = new List<Environment>
        {
            environments[0],
            environments[1],
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddRangeAsync(environments.AsQueryable());
        await context.SaveChangesAsync();
        
        var repository = new EnvironmentRepository(context);

        // Act
        var result = await repository.GetEnvironmentsAsync(user.Id);

        // Assert
        result.Should().BeEquivalentTo(expectedEnvironments);
    }
    
    [Fact]
    public async Task GetEnvironmentsAsync_EnvironmentsNotFound_ReturnsEmptyList()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environments = new List<Environment>
        {
            new()
                { Id = Guid.NewGuid(), Name = "Environment 1", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 2", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 3", Owner = new User { Id = Guid.NewGuid() } },
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddRangeAsync(environments.AsQueryable());
        await context.SaveChangesAsync();
        
        var repository = new EnvironmentRepository(context);

        // Act
        var result = await repository.GetEnvironmentsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEnvironmentAsync_EnvironmentFound_ReturnsEnvironment()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };

        var environmentId = Guid.NewGuid();
        
        var environments = new List<Environment>
        {
            new()
                { Id = Guid.NewGuid(), Name = "Environment 1", Owner = user },
            new()
                { Id = environmentId, Name = "Environment 2", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 3", Owner = new User { Id = Guid.NewGuid() } },
        };

        var expectedEnvironment = environments[1];
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddRangeAsync(environments.AsQueryable());
        await context.SaveChangesAsync();
        
        var repository = new EnvironmentRepository(context);

        // Act
        var result = await repository.GetEnvironmentAsync(environmentId, user.Id);

        // Assert
        result.Should().BeEquivalentTo(expectedEnvironment);
    }

    [Fact]
    public async Task GetEnvironmentAsync_EnvironmentNotFound_ReturnsNull()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };

        var environments = new List<Environment>
        {
            new()
                { Id = Guid.NewGuid(), Name = "Environment 1", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 2", Owner = user },
            new()
                { Id = Guid.NewGuid(), Name = "Environment 3", Owner = new User { Id = Guid.NewGuid() } },
        };

        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddRangeAsync(environments.AsQueryable());
        await context.SaveChangesAsync();
        
        var repository = new EnvironmentRepository(context);

        // Act
        var result = await repository.GetEnvironmentAsync(Guid.NewGuid(), user.Id);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateEnvironmentAsync_EnvironmentExists_UpdatesEnvironment()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        var updatedEnvironment = new Environment
        {
            Id = environmentId,
            Name = "Updated Environment 1",
            Owner = user,
        };
        
        // Act
        await repository.UpdateEnvironmentAsync(updatedEnvironment);
        
        // Assert
        (await context.Environments.SingleAsync(x => x.Id == environmentId)).Should().BeEquivalentTo(updatedEnvironment);
    }
    
    [Fact]
    public async Task DeleteEnvironmentAsync_EnvironmentExists_DeletesEnvironment()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        await repository.DeleteEnvironmentAsync(environmentId, user.Id);
        
        // Assert
        context.Environments.All(x => x.Id != environmentId).Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteEnvironmentAsync_EnvironmentNotFound_ReturnsWasDeletedFalse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.DeleteEnvironmentAsync(Guid.NewGuid(), user.Id);
        
        // Assert
        result.wasDeleted.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteEnvironmentAsync_EnvironmentNotFound_EnvironmentShouldNotBeDeleted()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        await repository.DeleteEnvironmentAsync(Guid.NewGuid(), user.Id);
        
        // Assert
        context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId).Should().NotBeNull();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_NameAvailable_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.IsNameAvailableAsync("Environment 2", user.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_NameNotAvailable_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.IsNameAvailableAsync("Environment 1", user.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_NameNotFoundAndNoteSameForEnvironment_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.IsNameAvailableAsync("Environment 2", environmentId, user.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_NameSameForEnvironment_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.IsNameAvailableAsync("Environment 1", environmentId, user.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_NameFoundOnOtherEnvironment_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var environmentId = Guid.NewGuid();
        
        var environment = new Environment
        {
            Id = environmentId,
            Name = "Environment 1",
            Owner = user,
        };
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var repository = new EnvironmentRepository(context);
        
        // Act
        var result = await repository.IsNameAvailableAsync("Environment 1", Guid.NewGuid(), user.Id);
        
        // Assert
        result.Should().BeFalse();
    }
}
