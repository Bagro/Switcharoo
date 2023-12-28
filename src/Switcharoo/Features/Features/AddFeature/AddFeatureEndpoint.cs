using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Database.Entities;
using Switcharoo.Extensions;

namespace Switcharoo.Features.Features.AddFeature;

public sealed class AddFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/feature", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("AddFeature")
            .WithOpenApi()
            .Produces<AddFeatureResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict);
    }
    
    public static async Task<IResult> HandleAsync(AddFeatureRequest request, ClaimsPrincipal user, IFeatureRepository featureRepository, IUserRepository userRepository, CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Name is required");
        }
        
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return Results.BadRequest("Key is required");
        }

        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Key = request.Key,
            Description = request.Description,
            Environments = [],
        };
        
        if (!await featureRepository.IsNameAvailableAsync(feature.Name, user.GetUserId()))
        {
            return Results.Conflict("Name is already in use");
        }

        if (string.IsNullOrWhiteSpace(feature.Key))
        {
            feature.Key = feature.Name.Replace(" ", "-").ToLower();
        }

        if (!await featureRepository.IsKeyAvailableAsync(feature.Key, user.GetUserId()))
        {
            return Results.Conflict("Key is already in use");
        }
        
        var storedUser = await userRepository.GetUserAsync(user.GetUserId());
        if (storedUser == null)
        {
            return Results.BadRequest("User not found");
        }
        
        feature.Owner = storedUser;

        if (request.Environments != null)
        {
            foreach (var environment in request.Environments)
            {
                var storedEnvironment = await featureRepository.GetEnvironmentAsync(environment.Id, user.GetUserId());
                if (storedEnvironment == null)
                {
                    continue;
                }

                feature.Environments.Add(
                    new FeatureEnvironment
                    {
                        Environment = storedEnvironment,
                        Feature = feature,
                        IsEnabled = environment.IsEnabled,
                    });
            }
        }

        await featureRepository.AddFeatureAsync(feature);
     
        return  Results.Ok(new AddFeatureResponse(feature.Name, feature.Id));
    }
}
