using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Database.Entities;
using Switcharoo.Features.Features;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Features;

public sealed class FeatureRepositoryTests
{
    private readonly DbContextOptions<SqliteDbContext> _dbContextOptions = new DbContextOptionsBuilder<SqliteDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDbForTesting")
        .EnableSensitiveDataLogging()
        .Options;
    
    [Fact]
    public async Task GetFeatureStateAsync_FeatureFound_ReturnsFeatureStateAndFoundIndicator()
    {
        // Arrange
        var feature = FeatureFakes.GetFakeFeature();
        feature.Environments[0].IsEnabled = true;
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetFeatureStateAsync(feature.Key, feature.Environments[0].Environment.Id);
        
        // Assert
        result.Should().Be((true, true));
    }
    
    [Fact]
    public async Task GetFeatureStateAsync_FeatureNotFound_ReturnsFeatureStateFalseAndFoundIndicator()
    {
        // Arrange
        var feature = FeatureFakes.GetFakeFeature();
        feature.Environments[0].IsEnabled = true;
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetFeatureStateAsync(feature.Key, Guid.NewGuid());
        
        // Assert
        result.Should().Be((false, false));
    }
    
    [Fact]
    public async Task ToggleFeatureAsync_FeatureFound_TogglesFeatureAndReturnsResult()
    {
        // Arrange
        var feature = FeatureFakes.GetFakeFeature();
        feature.Environments[0].IsEnabled = true;
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.ToggleFeatureAsync(feature.Id, feature.Environments[0].Environment.Id, feature.Owner.Id);
        
        // Assert
        result.Should().Be((false, true, "Feature toggled"));
    }
    
    [Fact]
    public async Task ToggleFeatureAsync_FeatureNotFound_ReturnsFalseAndReason()
    {
        // Arrange
        var feature = FeatureFakes.GetFakeFeature();
        feature.Environments[0].IsEnabled = true;
        
        var context = new SqliteDbContext(_dbContextOptions);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.ToggleFeatureAsync(Guid.NewGuid(), feature.Environments[0].Environment.Id, feature.Owner.Id);
        
        // Assert
        result.Should().Be((false, false, "Feature not found"));
    }
    
    [Fact]
    public async Task AddFeatureAsync_FeatureAdded_ShouldSaveFeature()
    {
        // Arrange
        var feature = FeatureFakes.GetFakeFeature();
        feature.Environments[0].IsEnabled = true;
       
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        await repository.AddFeatureAsync(feature);
        
        // Assert
        context.Features.Any(x => x.Id == feature.Id).Should().BeTrue();
    }
    
    [Fact]
    public async Task AddEnvironmentToFeatureAsync_FeatureEnvironmentAdded_ShouldSaveFeatureEnvironment()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var feature = FeatureFakes.GetFakeFeature();
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = feature.Owner;
        
        context.Features.Add(feature);
        context.Environments.Add(environment);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        await repository.AddEnvironmentToFeatureAsync(feature.Id, environment.Id, feature.Owner.Id);
        
        // Assert
        context.FeatureEnvironments.Any(x => x.Feature.Id == feature.Id && x.Environment.Id == environment.Id).Should().BeTrue();
    }
    
    [Fact]
    public async Task AddEnvironmentToFeatureAsync_FeatureEnvironmentAlreadyExists_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var feature = FeatureFakes.GetFakeFeature();
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = feature.Owner;
        
        feature.Environments.Add(new FeatureEnvironment
        {
            Id = Guid.NewGuid(),
            Feature = feature,
            Environment = environment,
            IsEnabled = false,
        });
        
        context.Features.Add(feature);
        context.Environments.Add(environment);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.AddEnvironmentToFeatureAsync(feature.Id, environment.Id, feature.Owner.Id);
        
        // Assert
        result.Should().Be((false, "Feature environment already exists"));
    }
    
    [Fact]
    public async Task AddEnvironmentToFeatureAsync_FeatureNotFound_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = UserFakes.GetFakeUser();
        
        context.Environments.Add(environment);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.AddEnvironmentToFeatureAsync(Guid.NewGuid(), environment.Id, environment.Owner.Id);
        
        // Assert
        result.Should().Be((false, "Feature or environment not found"));
    }
    
    [Fact]
    public async Task AddEnvironmentToFeatureAsync_EnvironmentNotFound_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var feature = FeatureFakes.GetFakeFeature();
        feature.Owner = UserFakes.GetFakeUser();
        
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.AddEnvironmentToFeatureAsync(feature.Id, Guid.NewGuid(), feature.Owner.Id);
        
        // Assert
        result.Should().Be((false, "Feature or environment not found"));
    }
    
    [Fact]
    public async Task DeleteFeatureAsync_FeatureNotFound_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.DeleteFeatureAsync(Guid.NewGuid(), Guid.NewGuid());
        
        // Assert
        result.Should().Be((false, "Feature not found"));
    }
    
    [Fact]
    public async Task DeleteFeatureAsync_FeatureDeleted_ShouldDeleteFeature()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var feature = FeatureFakes.GetFakeFeature();
        
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        await repository.DeleteFeatureAsync(feature.Id, feature.Owner.Id);
        
        // Assert
        context.Features.Any(x => x.Id == feature.Id).Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteEnvironmentFromFeatureAsync_FeatureNotFound_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.DeleteEnvironmentFromFeatureAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        
        // Assert
        result.Should().Be((false, "Feature not found"));
    }
    
    [Fact]
    public async Task DeleteEnvironmentFromFeatureAsync_EnvironmentNotFound_ShouldReturnFalseAndReason()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.DeleteEnvironmentFromFeatureAsync(feature.Id, Guid.NewGuid(), feature.Owner.Id);
        
        // Assert
        result.Should().Be((false, "Feature environment not found"));
    }
    
    [Fact]
    public async Task DeleteEnvironmentFromFeatureAsync_EnvironmentDeleted_ShouldDeleteEnvironment()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var feature = FeatureFakes.GetFakeFeature();
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = feature.Owner;
        
        var featureEnvironmentId = Guid.NewGuid();
        feature.Environments.Add(new FeatureEnvironment
        {
            Id = featureEnvironmentId,
            Feature = feature,
            Environment = environment,
            IsEnabled = false,
        });
        
        context.Features.Add(feature);
        context.Environments.Add(environment);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        await repository.DeleteEnvironmentFromFeatureAsync(feature.Id, environment.Id, feature.Owner.Id);
        
        // Assert
        context.FeatureEnvironments.Any(x => x.Id == featureEnvironmentId).Should().BeFalse();
    }
    
    [Fact]
    public async Task GetFeaturesAsync_FeatureNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetFeaturesAsync(Guid.NewGuid());
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetFeaturesAsync_FeatureFound_ShouldReturnFeature()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetFeaturesAsync(feature.Owner.Id);
        
        // Assert
        result.Should().Contain(feature).And.HaveCount(1);
    }
    
    [Fact]
    public async Task GetFeatureAsync_FeatureNotFound_ShouldReturnNull()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetFeatureAsync(Guid.NewGuid(), Guid.NewGuid());
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetFeatureAsync_FeatureFound_ShouldReturnFeature()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetFeatureAsync(feature.Id, feature.Owner.Id);
        
        // Assert
        result.Should().BeEquivalentTo(feature);
    }
    
    [Fact]
    public async Task UpdateFeatureAsync_FeatureNameChanged_ShouldUpdateFeatureName()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        
        var newName = "New Name";
        feature.Name = newName;
        // Act
        await repository.UpdateFeatureAsync(feature);
        
        // Assert
        (await context.Features.SingleAsync(x => x.Id == feature.Id)).Name.Should().Be(newName);
        feature.Name = "New Name";
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_FeatureNameAvailable_ShouldReturnTrue()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync("Different Name", feature.Owner.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_FeatureNameNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync(feature.Name, feature.Owner.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_UpdatingFeatureNameNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        var secondFeature = FeatureFakes.GetFakeFeature();
        secondFeature.Owner = feature.Owner;
        context.Features.Add(secondFeature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync(secondFeature.Name, feature.Id, feature.Owner.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_UpdatingFeatureNameAvailable_ShouldReturnTrue()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        var secondFeature = FeatureFakes.GetFakeFeature();
        secondFeature.Owner = feature.Owner;
        context.Features.Add(secondFeature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync("Different Name", feature.Id, feature.Owner.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsKeyAvailableAsync_FeatureKeyAvailable_ShouldReturnTrue()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsKeyAvailableAsync("Different Key", feature.Owner.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsKeyAvailableAsync_FeatureKeyNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        
        var secondFeature = FeatureFakes.GetFakeFeature();
        secondFeature.Owner = feature.Owner;
        context.Features.Add(secondFeature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsKeyAvailableAsync(secondFeature.Key, feature.Owner.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsKeyAvailableAsync_UpdatingFeatureKeyAvailable_ShouldReturnTrue()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        var secondFeature = FeatureFakes.GetFakeFeature();
        secondFeature.Owner = feature.Owner;
        context.Features.Add(secondFeature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsKeyAvailableAsync("New Key", feature.Id, feature.Owner.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsKeyAvailableAsync_UpdatingFeatureKeyNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        var feature = FeatureFakes.GetFakeFeature();
        context.Features.Add(feature);
        var secondFeature = FeatureFakes.GetFakeFeature();
        secondFeature.Owner = feature.Owner;
        context.Features.Add(secondFeature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsKeyAvailableAsync(secondFeature.Key, feature.Id, feature.Owner.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetEnvironmentAsync_EnvironmentFound_ShouldReturnEnvironment()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = UserFakes.GetFakeUser();
        context.Environments.Add(environment);
        await context.SaveChangesAsync();
        
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetEnvironmentAsync(environment.Id, environment.Owner.Id);
        
        // Assert
        result.Should().BeEquivalentTo(environment);
    }
    
    [Fact]
    public async Task GetEnvironmentAsync_EnvironmentNotFound_ShouldReturnNull()
    {
        // Arrange
        var context = new SqliteDbContext(_dbContextOptions);
        var repository = new FeatureRepository(context);
        
        // Act
        var result = await repository.GetEnvironmentAsync(Guid.NewGuid(), Guid.NewGuid());
        
        // Assert
        result.Should().BeNull();
    }
}
