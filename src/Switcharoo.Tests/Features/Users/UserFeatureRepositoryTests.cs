using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Features.Users;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Users;

public sealed class UserFeatureRepositoryTests
{
    private readonly DbContextOptions<SqliteDbContext> _dbContextOptions = new DbContextOptionsBuilder<SqliteDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDbForTesting")
        .EnableSensitiveDataLogging()
        .Options;

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetUserAsync(user.Id);

        // Assert
        result.Should().BeEquivalentTo(user);
    }
    
    [Fact]
    public async Task GetUserAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var user = UserFakes.GetFakeUser();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetUserAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTeamInviteAsync_WhenTeamInviteExists_ReturnsTeamInvite()
    {
        // Arrange
        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.TeamInvites.AddAsync(teamInvite);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetTeamInviteAsync(teamInvite.InviteCode);

        // Assert
        result.Should().BeEquivalentTo(teamInvite);
    }
    
    [Fact]
    public async Task GetTeamInviteAsync_WhenTeamInviteDoesNotExist_ReturnsNull()
    {
        // Arrange
        var teamInvite = TeamInviteFakes.GetFakeTeamInvite();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.TeamInvites.AddAsync(teamInvite);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetTeamInviteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTeamAsync_WhenTeamExists_ReturnsTeam()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetTeamAsync(team.Id);

        // Assert
        result.Should().BeEquivalentTo(team);
    }
    
    [Fact]
    public async Task GetTeamAsync_WhenTeamDoesNotExist_ReturnsNull()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        var result = await userFeatureRepository.GetTeamAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateTeamAsync_WhenTeamExists_UpdatesTeam()
    {
        // Arrange
        var team = TeamFakes.GetFakeTeam();

        await using var context = new SqliteDbContext(_dbContextOptions);
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();

        var userFeatureRepository = new UserFeatureRepository(context);

        // Act
        team.Name = "New Name";
        await userFeatureRepository.UpdateTeamAsync(team);
        var result = await context.Teams.SingleOrDefaultAsync(x => x.Id == team.Id);

        // Assert
        result.Should().BeEquivalentTo(team);
    }
}
