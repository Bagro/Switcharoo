using System.Reflection;
using Switcharoo.Common;

namespace Switcharoo.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => typeof(IEndpoint).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        foreach (var type in types)
        {
            var endpoint = Activator.CreateInstance(type) as IEndpoint;
            endpoint?.MapEndpoint(endpoints);
        }
    }
}
