using EmailAttachments.McpServer;
using ExtractLoadInvoices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace EmailAttachments.McpServer.AspNetCore;

/// <summary>
/// Extension methods for registering and mapping the Gmail MCP server in ASP.NET Core applications.
/// </summary>
public static class GmailMcpAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Gmail email attachment services and the MCP server with the HTTP/SSE transport
    /// for use in an ASP.NET Core application. Call <see cref="MapGmailMcpServer"/> on the
    /// <see cref="IEndpointRouteBuilder"/> to expose the MCP endpoint.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration used to bind Gmail and storage settings.</param>
    /// <param name="serverName">The MCP server name advertised to clients.</param>
    /// <param name="serverVersion">The MCP server version advertised to clients.</param>
    /// <returns>The <see cref="IMcpServerBuilder"/> for further MCP server customization.</returns>
    public static IMcpServerBuilder AddGmailMcpServer(
        this IServiceCollection services,
        IConfiguration configuration,
        string serverName = "Gmail Email Attachments MCP Server",
        string serverVersion = "1.0.0")
    {
        services.AddGmailFilesDownloader(configuration);

        return services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = serverName,
                    Version = serverVersion
                };
            })
            .WithHttpTransport()
            .WithGmailMcpTools();
    }

    /// <summary>
    /// Maps the MCP endpoint at the specified route pattern.
    /// Must be called after <see cref="AddGmailMcpServer"/> has been used during service registration.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The URL pattern at which the MCP endpoint is exposed (default: <c>/mcp</c>).</param>
    /// <returns>A convention builder for the mapped MCP endpoint.</returns>
    public static IEndpointConventionBuilder MapGmailMcpServer(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/mcp")
        => endpoints.MapMcp(pattern);
}
