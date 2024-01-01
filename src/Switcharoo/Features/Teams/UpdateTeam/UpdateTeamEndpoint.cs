using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Teams.UpdateTeam;

public sealed class UpdateTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/team", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("UpdateTeam")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status409Conflict);
    }

    public static async Task<IResult> HandleAsync(UpdateTeamRequest request, ClaimsPrincipal user, ITeamRepository teamRepository, CancellationToken cancellationToken)
    {
        var storedUser = await teamRepository.GetUserAsync(user.GetUserId());

        if (storedUser is null)
        {
            return Results.BadRequest("User not found");
        }

        if (!await teamRepository.IsNameAvailableAsync(request.Name, request.Id))
        {
            return Results.Conflict("Name is already in use");
        }

        var team = await teamRepository.GetTeamAsync(request.Id);

        if (team is null)
        {
            return Results.BadRequest("Team not found");
        }

        if (!UserAllowedToUpdateTeam(storedUser, team))
        {
            return Results.Forbid();
        }

        var updateTeamResult = await UpdateTeamWithRequest(team, request, storedUser, teamRepository);

        await teamRepository.UpdateTeamAsync(team);
        
        return updateTeamResult.allEnvironmentsUpdated && updateTeamResult.allFeaturesUpdated
            ? Results.Ok("Team updated")
            : Results.Ok("Team updated, but some environments or features could not be updated due to the user not having access to them");
    }

    private static bool UserAllowedToUpdateTeam(User user, Team team)
    {
        return team.Owner.Id == user.Id || (team.AllCanManage && team.Members.Exists(x => x.Id == user.Id));
    }

    private static async Task<(bool allEnvironmentsUpdated, bool allFeaturesUpdated)> UpdateTeamWithRequest(Team team, UpdateTeamRequest request, User user, ITeamRepository teamRepository)
    {
        team.Name = request.Name;
        team.Description = request.Description;
        team.AllCanManage = request.AllCanManage;
        team.InviteOnly = request.InviteOnly;

        var allEnvironmentsAdded = await UpdateTeamEnvironments(request, team, user, teamRepository);

        var allFeaturesAdded = await UpdateTeamFeatures(request, team, user, teamRepository);
        
        return (allEnvironmentsAdded, allFeaturesAdded);
    }

    private static async Task<bool> UpdateTeamFeatures(UpdateTeamRequest request, Team team, User user, ITeamRepository teamRepository)
    {
        var requestedFeatureIds = request.Features.Select(x => x.FeatureId).ToList();
        var existingFeatures = team.Features.Select(x => x.Feature.Id).ToList();
        var featuresToAdd = requestedFeatureIds.Except(existingFeatures).ToList();
        var featuresToRemove = existingFeatures.Except(requestedFeatureIds).ToList();

        foreach (var featureId in featuresToRemove)
        {
            team.Features.Remove(team.Features.Single(x => x.Feature.Id == featureId));
        }

        var features = await teamRepository.GetFeaturesForIdAsync(featuresToAdd, user.Id);

        foreach (var feature in features)
        {
            team.Features.Add(
                new TeamFeature
                {
                    Team = team,
                    Feature = feature,
                    IsReadOnly = request.Features.Single(x => x.FeatureId == feature.Id).IsReadOnly,
                    AllCanToggle = request.Features.Single(x => x.FeatureId == feature.Id).AllCanToggle
                });
        }

        existingFeatures = team.Features.Select(x => x.Feature.Id).ToList();
        var count = requestedFeatureIds.Except(existingFeatures).Count();
        
        return count == 0;
    }

    private static async Task<bool> UpdateTeamEnvironments(UpdateTeamRequest request, Team team, User user, ITeamRepository teamRepository)
    {
        var requestedEnvironmentIds = request.Environments.Select(x => x.EnvironmentId).ToList();
        var existingEnvironments = team.Environments.Select(x => x.Environment.Id).ToList();
        var environmentsToAdd = requestedEnvironmentIds.Except(existingEnvironments).ToList();
        var environmentsToRemove = existingEnvironments.Except(requestedEnvironmentIds).ToList();

        foreach (var environmentId in environmentsToRemove)
        {
            team.Environments.Remove(team.Environments.Single(x => x.Environment.Id == environmentId));
        }

        var environments = await teamRepository.GetEnvironmentsForIdAsync(environmentsToAdd, user.Id);

        foreach (var environment in environments)
        {
            team.Environments.Add(
                new TeamEnvironment
                {
                    Team = team,
                    Environment = environment,
                    IsReadOnly = request.Environments.Single(x => x.EnvironmentId == environment.Id).IsReadOnly,
                });
        }

        existingEnvironments = team.Environments.Select(x => x.Environment.Id).ToList();
        var count = requestedEnvironmentIds.Except(existingEnvironments).Count();

        return count == 0;
    }
}
