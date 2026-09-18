using Microsoft.AspNetCore.Routing;

namespace Records.Shared.Infra.Http;

/// <summary>
/// Represents the marker interface for endpoints.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Adds the RouteEndpoint to the IEndpointRouteBuilder.
    /// </summary>
    /// <param name="app">The web application used to configure the HTTP pipeline, and routes.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
