using System.Security.Claims;
using Switcharoo.Common;
using Switcharoo.Extensions;
using Switcharoo.Features.Features.Model;

namespace Switcharoo.Features.Features.GetFeature;

public sealed class GetFeatureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/feature/{id:guid}", handler: HandleAsync)
            .RequireAuthorization()
            .WithName("GetFeature")
            .WithOpenApi()
            .Produces<Feature>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, IFeatureRepository featureRepository, CancellationToken cancellationToken)
    {
        var feature = await featureRepository.GetFeatureAsync(id, user.GetUserId());

        if (feature is null)
        {
            return Results.NotFound("Feature not found");
        }

        var returnFeature = new Model.Feature
        {
            Id = feature.Id,
            Name = feature.Name,
            Key = feature.Key,
            Description = feature.Description,
            Environments = feature.Environments.OrderBy(e => e.Environment.Name).Select(y => new FeatureEnvironment(y.IsEnabled, y.Environment.Name, y.Environment.Id)).ToList()
        };
        
            return Results.Ok(returnFeature);
        }
    }
