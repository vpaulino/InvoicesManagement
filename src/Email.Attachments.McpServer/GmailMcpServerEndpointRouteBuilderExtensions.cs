using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace EmailAttachments.McpServer;

/// <summary>
/// Extension methods for mapping the Gmail MCP server endpoint onto an
/// ASP.NET <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static class GmailMcpServerEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the Gmail MCP Streamable-HTTP endpoint at the specified route
    /// pattern. The endpoint accepts both GET (SSE streaming) and POST
    /// (individual JSON-RPC messages) requests.
    /// </summary>
    /// <param name="endpoints">The route builder to attach the endpoint to.</param>
    /// <param name="pattern">
    /// The URL path prefix for MCP requests. Defaults to <c>/mcp</c>.
    /// </param>
    /// <returns>
    /// An <see cref="IEndpointConventionBuilder"/> that can be used to
    /// further configure the endpoint (e.g. <c>.RequireAuthorization()</c>).
    /// </returns>
    /// <example>
    /// <code>
    /// // Basic usage — unauthenticated:
    /// app.MapGmailMcpServer("/mcp");
    ///
    /// // Require the caller to be authenticated:
    /// app.MapGmailMcpServer("/mcp").RequireAuthorization();
    /// </code>
    /// </example>
    public static IEndpointConventionBuilder MapGmailMcpServer(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/mcp")
    {
        return endpoints.MapMcp(pattern);
    }
}
