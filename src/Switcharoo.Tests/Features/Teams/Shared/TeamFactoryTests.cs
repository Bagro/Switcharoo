using FluentAssertions;
using Switcharoo.Features.Teams.Model;
using Switcharoo.Features.Teams.Shared;
using Switcharoo.Tests.Common;
using Xunit;

namespace Switcharoo.Tests.Features.Teams.Shared;

public sealed class TeamFactoryTests
{
    [Fact]
    public void CreateFromEntity_UserNotOwnerOrMember_ReturnsTeamWithoutMembers()
    {
        // Arrange
        var teamEntity = TeamFakes.GetFakeTeam();
        teamEntity.Members.Add(UserFakes.GetFakeUser());

        var expectedTeam = new Team
        {
            Id = teamEntity.Id,
            Name = teamEntity.Name,
            Description = teamEntity.Description,
            AllCanManage = teamEntity.AllCanManage,
            InviteOnly = teamEntity.InviteOnly,
        };
        
        foreach (var teamEnvironment in teamEntity.Environments)
        {
            expectedTeam.Environments.Add(new TeamEnvironment(teamEnvironment.Id, teamEnvironment.Environment.Name, teamEnvironment.IsReadOnly));
        }
        
        foreach (var teamFeature in teamEntity.Features)
        {
            expectedTeam.Features.Add(new TeamFeature(teamFeature.Id, teamFeature.Feature.Name, teamFeature.IsReadOnly, teamFeature.AllCanToggle));
        }

        // Act
        var team = TeamFactory.CreateFromEntity(teamEntity, Guid.Empty);

        // Assert
        team.Should().BeEquivalentTo(expectedTeam);
    }
    
    [Fact]
    public void CreateFromEntity_UserIaOwner_ReturnsTeamWithMembers()
    {
        // Arrange
        var teamEntity = TeamFakes.GetFakeTeam();
        teamEntity.Members.Add(UserFakes.GetFakeUser());
        
        var expectedTeam = new Team
        {
            Id = teamEntity.Id,
            Name = teamEntity.Name,
            Description = teamEntity.Description,
            AllCanManage = teamEntity.AllCanManage,
            InviteOnly = teamEntity.InviteOnly,
        };
        
        foreach (var teamEnvironment in teamEntity.Environments)
        {
            expectedTeam.Environments.Add(new TeamEnvironment(teamEnvironment.Id, teamEnvironment.Environment.Name, teamEnvironment.IsReadOnly));
        }
        
        foreach (var teamFeature in teamEntity.Features)
        {
            expectedTeam.Features.Add(new TeamFeature(teamFeature.Id, teamFeature.Feature.Name, teamFeature.IsReadOnly, teamFeature.AllCanToggle));
        }
        
        foreach (var teamMember in teamEntity.Members)
        {
            expectedTeam.Members.Add(new TeamMember(teamMember.Id, teamMember.UserName ?? string.Empty, teamMember.Email ?? string.Empty));
        }

        // Act
        var team = TeamFactory.CreateFromEntity(teamEntity, teamEntity.Owner.Id);

        // Assert
        team.Should().BeEquivalentTo(expectedTeam);
    }
    
    [Fact]
    public void CreateFromEntity_UserIaOMember_ReturnsTeamWithMembers()
    {
        // Arrange
        var teamEntity = TeamFakes.GetFakeTeam();
        teamEntity.Members.Add(UserFakes.GetFakeUser());
        
        var expectedTeam = new Team
        {
            Id = teamEntity.Id,
            Name = teamEntity.Name,
            Description = teamEntity.Description,
            AllCanManage = teamEntity.AllCanManage,
            InviteOnly = teamEntity.InviteOnly,
        };
        
        foreach (var teamEnvironment in teamEntity.Environments)
        {
            expectedTeam.Environments.Add(new TeamEnvironment(teamEnvironment.Id, teamEnvironment.Environment.Name, teamEnvironment.IsReadOnly));
        }
        
        foreach (var teamFeature in teamEntity.Features)
        {
            expectedTeam.Features.Add(new TeamFeature(teamFeature.Id, teamFeature.Feature.Name, teamFeature.IsReadOnly, teamFeature.AllCanToggle));
        }
        
        foreach (var teamMember in teamEntity.Members)
        {
            expectedTeam.Members.Add(new TeamMember(teamMember.Id, teamMember.UserName ?? string.Empty, teamMember.Email ?? string.Empty));
        }

        // Act
        var team = TeamFactory.CreateFromEntity(teamEntity, teamEntity.Members[0].Id);

        // Assert
        team.Should().BeEquivalentTo(expectedTeam);
    }
}
