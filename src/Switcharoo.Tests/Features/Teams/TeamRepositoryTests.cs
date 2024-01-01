using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Features.Teams;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams;

public sealed class TeamRepositoryTests
{
    private readonly DbContextOptions<SqliteDbContext> _dbContextOptions = new DbContextOptionsBuilder<SqliteDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDbForTesting")
        .EnableSensitiveDataLogging()
        .Options;
    
    [Fact]
    public async Task AddTeamAsync_WhenCalled_AddsTeamToDatabase()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        
        // Act
        await repository.AddTeamAsync(team);
        
        // Assert
        var storedTeam = await context.Teams.SingleOrDefaultAsync(x => x.Id == team.Id);
        storedTeam.Should().BeEquivalentTo(team);
    }
    
    [Fact]
    public async Task UpdateTeamAsync_WhenCalled_UpdatesTeamInDatabase()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        team.Name = "New Name";
        await repository.UpdateTeamAsync(team);
        
        // Assert
        var storedTeam = await context.Teams.SingleOrDefaultAsync(x => x.Id == team.Id);
        storedTeam.Should().BeEquivalentTo(team);
    }
    
    [Fact]
    public async Task DeleteTeamAsync_WhenCalled_DeletesTeamFromDatabase()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        await repository.DeleteTeamAsync(team);
        
        // Assert
        var storedTeam = await context.Teams.SingleOrDefaultAsync(x => x.Id == team.Id);
        storedTeam.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTeamAsync_WhenCalled_ReturnsTeamFromDatabase()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var storedTeam = await repository.GetTeamAsync(team.Id);
        
        // Assert
        storedTeam.Should().BeEquivalentTo(team);
    }
    
    [Fact]
    public async Task GetTeamAsync_WhenCalledWithInvalidTeamId_ReturnsNull()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var storedTeam = await repository.GetTeamAsync(Guid.NewGuid());
        
        // Assert
        storedTeam.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTeamsAsync_WhenCalled_ReturnsTeamsFromDatabase()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetTeamsAsync(user.Id);
        
        // Assert
        result.teams.Should().ContainEquivalentOf(team);
    }
    
    [Fact]
    public async Task GetTeamsAsync_WhenCalledWithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetTeamsAsync(Guid.NewGuid());
        
        // Assert
        result.teams.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetEnvironmentsForIdAsync_WhenCalled_ReturnsEnvironmentsFromDatabase()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetEnvironmentsForIdAsync(new List<Guid> { environment.Id }, user.Id);
        
        // Assert
        result.Should().ContainEquivalentOf(environment);
    }
    
    [Fact]
    public async Task GetEnvironmentsForIdAsync_WhenCalledWithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var environment = EnvironmentFakes.GetFakeEnvironment();
        environment.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Environments.AddAsync(environment);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetEnvironmentsForIdAsync(new List<Guid> { environment.Id }, Guid.NewGuid());
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_WhenAvailable_ReturnsTrue()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync("Team Name");
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_WhenNotAvailable_ReturnsFalse()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync(team.Name);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_WhenAvailableOnUpdate_ReturnsTrue()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync(team.Name, team.Id);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsNameAvailableAsync_WhenNotAvailableOnUpdate_ReturnsFalse()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var team = TeamFakes.GetFakeTeam();
        team.Owner = user;
        var team2 = TeamFakes.GetFakeTeam();
        team2.Owner = user;
        team2.Name = "Team 2";
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Teams.AddRangeAsync(team, team2);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.IsNameAvailableAsync(team2.Name, team.Id);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetFeaturesForIdAsync_WhenCalled_ReturnsFeatures()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var feature = FeatureFakes.GetFakeFeature();
        feature.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetFeaturesForIdAsync(new List<Guid> { feature.Id }, user.Id);
        
        // Assert
        result.Should().ContainEquivalentOf(feature);
    }
    
    [Fact]
    public async Task GetFeaturesForIdAsync_WhenCalledWithInvalidUserId_ReturnsEmptyList()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var feature = FeatureFakes.GetFakeFeature();
        feature.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetFeaturesForIdAsync(new List<Guid> { feature.Id }, Guid.NewGuid());
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetFeaturesForIdAsync_WhenCalledWithInvalidFeatureId_ReturnsEmptyList()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();
        var feature = FeatureFakes.GetFakeFeature();
        feature.Owner = user;
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        await context.Features.AddAsync(feature);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetFeaturesForIdAsync(new List<Guid> { Guid.NewGuid() }, user.Id);
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task AddInviteCodeAsync_WhenCalled_AddsInviteCodeToDatabase()
    {
        // Arrange
        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();
        await using var context = new SqliteDbContext(_dbContextOptions);
        var repository = new TeamRepository(context);
        
        // Act
        await repository.AddInviteCodeAsync(teamInvite);
        
        // Assert
        var storedTeamInvite = await context.TeamInvites.SingleOrDefaultAsync(x => x.InviteCode == teamInvite.InviteCode);
        storedTeamInvite.Should().BeEquivalentTo(teamInvite);
    }
}
