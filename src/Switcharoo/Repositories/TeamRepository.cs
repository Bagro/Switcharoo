using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Interfaces;
using Switcharoo.Model.Requests;
using Team = Switcharoo.Database.Entities.Team;
using TeamEnvironment = Switcharoo.Database.Entities.TeamEnvironment;
using TeamFeature = Switcharoo.Database.Entities.TeamFeature;

namespace Switcharoo.Repositories;

public sealed class TeamRepository(BaseDbContext context) : ITeamRepository
{
    public async Task<(bool wasAdded, Guid id, string reason)> AddTeamAsync(AddTeamRequest request, Guid userId)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            AllCanManage = request.AllCanManage,
            InviteOnly = request.InviteOnly,
            Owner = user,
        };
        
        await context.Teams.AddAsync(team);
        await context.SaveChangesAsync();
        
        return (true, team.Id, "Team added");
    }

    public async Task<(bool wasUpdated, string reason)> UpdateTeamAsync(UpdateTeamRequest request, Guid userId)
    {
        var team = await context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .SingleOrDefaultAsync(x => x.Id == request.Id);

        if (team == null)
        {
            return (false, "Team not found");
        }

        if (team.Owner.Id != userId && (!team.AllCanManage || !team.Members.Exists(x => x.Id == userId)))
        {
            return (false, "You don't have permission to update this team");
        }

        team.Name = request.Name;
        team.Description = request.Description;
        team.AllCanManage = request.AllCanManage;
        team.InviteOnly = request.InviteOnly;

        await UpdateTeamEnvironments(request, team);
        await UpdateTeamFeatures(request, team);

        await context.SaveChangesAsync();
        
        return (true, "Team updated");
    }

    public async Task<(bool wasDeleted, string reason)> DeleteTeamAsync(DeleteTeamRequest request, Guid userId)
    {
        var team =  await context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .SingleOrDefaultAsync(x => x.Id == request.TeamId);
        
        if (team == null)
        {
            return (false, "Team not found");
        }
        
        if (team.Owner.Id != userId)
        {
            return (false, "You don't have permission to delete this team");
        }
        
        context.Teams.Remove(team);
        
        await context.SaveChangesAsync();
        
        return (true, "Team deleted");
    }

    public async Task<(bool wasFound, Model.Team? team, string reason)> GetTeamAsync(Guid teamId, Guid userId)
    {
        var team = await context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .SingleOrDefaultAsync(x => x.Id == teamId);

        if (team is null)
        {
            return (false, null, "Team not found");
        }
        
        if (team.Owner.Id != userId && (!team.AllCanManage || !team.Members.Exists(x => x.Id == userId)))
        {
            return (false, null, "You don't have permission to view this team");
        }

        var returnTeam = CreateReturnTeam(team);

        AddTeamMembersToReturnTeam(team, returnTeam);
        AddEnvironmentsToReturnTeam(team, returnTeam);
        AddFeaturesToReturnTeam(team, returnTeam);
        
        return (true, returnTeam, "Team found");
    }

    public async Task<(bool wasFound, List<Model.Team> teams, string reason)> GetTeamsAsync(Guid userId)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        
        if (user is null)
        {
            return (false, new List<Model.Team>(), "User not found");
        }
        
        var teams = await context.Teams
            .Include(x => x.Owner).Include(x => x.Members)
            .Include(x => x.Environments).ThenInclude(teamEnvironment => teamEnvironment.Environment)
            .Include(x => x.Features).ThenInclude(teamFeature => teamFeature.Feature)
            .Where(x => x.Owner.Id == userId || !x.InviteOnly || x.Members.Contains(user))
            .ToListAsync();
        
        if(teams.Count == 0)
        {
            return (false, [], "No teams found");
        }
        
        var returnTeams = new List<Model.Team>();
        foreach (var team in teams)
        {
            var returnTeam = CreateReturnTeam(team);

            if (team.Owner.Id == userId || team.Members.Contains(user))
            {
                AddTeamMembersToReturnTeam(team, returnTeam);
            }
            
            AddEnvironmentsToReturnTeam(team, returnTeam);
            AddFeaturesToReturnTeam(team, returnTeam);
            
            returnTeams.Add(returnTeam);
        }
        
        return (true, returnTeams, "Teams found");
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid userId)
    {
        return !await context.Teams.AnyAsync(x => x.Name == name);
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid teamId, Guid userId)
    {
        return !await context.Teams.AnyAsync(x => x.Id != teamId && x.Name == name);
    }
    
    private static Model.Team CreateReturnTeam(Team team)
    {
        var returnTeam = new Model.Team
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            AllCanManage = team.AllCanManage,
            InviteOnly = team.InviteOnly,
        };
        return returnTeam;
    }

    private static void AddFeaturesToReturnTeam(Team team, Model.Team returnTeam)
    {
        foreach (var teamFeature in team.Features)
        {
            returnTeam.Features.Add(new Model.TeamFeature(teamFeature.Id, teamFeature.Feature.Name, teamFeature.IsReadOnly, teamFeature.AllCanToggle));
        }
    }

    private static void AddEnvironmentsToReturnTeam(Team team, Model.Team returnTeam)
    {
        foreach (var teamEnvironment in team.Environments)
        {
            returnTeam.Environments.Add(new Model.TeamEnvironment(teamEnvironment.Id, teamEnvironment.Environment.Name, teamEnvironment.IsReadOnly));
        }
    }

    private static void AddTeamMembersToReturnTeam(Team team, Model.Team returnTeam)
    {
        foreach (var teamMember in team.Members)
        {
            returnTeam.Members.Add(new Model.TeamMember(teamMember.Id, teamMember.UserName ?? string.Empty, teamMember.Email ?? string.Empty));
        }
    }
    
    private async Task UpdateTeamEnvironments(UpdateTeamRequest request, Team team)
    {
        var requestedEnvironmentIds = request.Environments.Select(x => x.EnvironmentId).ToList();
        var existingEnvironments = team.Environments.Select(x => x.Environment.Id).ToList();
        var environmentsToAdd = requestedEnvironmentIds.Except(existingEnvironments).ToList();
        var environmentsToRemove = existingEnvironments.Except(requestedEnvironmentIds).ToList();

        foreach (var environmentId in environmentsToAdd)
        {
            var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId);
            if (environment == null)
            {
                continue;
            }

            team.Environments.Add(new TeamEnvironment { Team = team, Environment = environment });
        }
        
        foreach (var environmentId in environmentsToRemove)
        {
            var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environmentId);
            if (environment == null)
            {
                continue;
            }

            team.Environments.Remove(new TeamEnvironment { Team = team, Environment = environment });
        }

        foreach (var teamEnvironment in team.Environments)
        {
            teamEnvironment.IsReadOnly = request.Environments.Single(x => x.EnvironmentId == teamEnvironment.Environment.Id).IsReadOnly;
        }
    }

    private async Task UpdateTeamFeatures(UpdateTeamRequest request, Team team)
    {
        var requestFeatureIds = request.Features.Select(x => x.FeatureId).ToList();
        var existingFeatureIds = team.Features.Select(x => x.Id).ToList();
        var featuresToAdd = requestFeatureIds.Except(existingFeatureIds).ToList();
        var featuresToRemove = existingFeatureIds.Except(requestFeatureIds).ToList();
        
        foreach (var featureId in featuresToAdd)
        {
            var feature = await context.Features.SingleOrDefaultAsync(x => x.Id == featureId);
            if (feature == null)
            {
                continue;
            }

            team.Features.Add(new TeamFeature { Team = team, Feature = feature });
        }
        
        foreach (var featureId in featuresToRemove)
        {
            var feature = await context.Features.SingleOrDefaultAsync(x => x.Id == featureId);
            if (feature == null)
            {
                continue;
            }

            team.Features.Remove(new TeamFeature { Team = team, Feature = feature });
        }
        
        foreach (var teamFeature in team.Features)
        {
            var requestFeature = request.Features.Single(x => x.FeatureId == teamFeature.Feature.Id);
            
            teamFeature.IsReadOnly = requestFeature.IsReadOnly;
            teamFeature.AllCanToggle = requestFeature.AllCanToggle;
        }
    }
}
