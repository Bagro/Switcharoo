using Switcharoo.Features.Teams.Model;

namespace Switcharoo.Features.Teams.Shared;

public static class TeamFactory
{
    public static Team CreateFromEntity(Database.Entities.Team entity, Guid userId)
    {
        var team = new Team
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            AllCanManage = entity.AllCanManage,
            InviteOnly = entity.InviteOnly,
        };
        
        
        AddEnvironmentsToReturnTeam(entity, team);
        AddFeaturesToReturnTeam(entity, team);
        
        if (entity.Owner.Id == userId || team.Members.Exists(x => x.Id == userId))
        {
            AddTeamMembersToReturnTeam(entity, team);
        }
        
        return team;
    }
    
    private static void AddFeaturesToReturnTeam(Database.Entities.Team entity, Team team)
    {
        foreach (var teamFeature in entity.Features)
        {
            team.Features.Add(new TeamFeature(teamFeature.Id, teamFeature.Feature.Name, teamFeature.IsReadOnly, teamFeature.AllCanToggle));
        }
    }

    private static void AddEnvironmentsToReturnTeam(Database.Entities.Team entity, Team team)
    {
        foreach (var teamEnvironment in entity.Environments)
        {
            team.Environments.Add(new TeamEnvironment(teamEnvironment.Id, teamEnvironment.Environment.Name, teamEnvironment.IsReadOnly));
        }
    }

    private static void AddTeamMembersToReturnTeam(Database.Entities.Team entity, Team team)
    {
        foreach (var teamMember in entity.Members)
        {
            team.Members.Add(new TeamMember(teamMember.Id, teamMember.UserName ?? string.Empty, teamMember.Email ?? string.Empty));
        }
    }
}
