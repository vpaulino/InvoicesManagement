using EmailAttachments.McpServer;
using ExtractLoadInvoices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace EmailAttachments.McpServer.Stdio;

/// <summary>
/// Extension methods for registering the Gmail MCP server with the stdio transport
/// for use in console / CLI host applications.
/// </summary>
public static class GmailMcpStdioServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Gmail email attachment services and the MCP server with the stdio transport.
    /// The host must run <c>await app.RunAsync()</c> (or equivalent) to start accepting MCP
    /// messages on standard input/output.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration used to bind Gmail and storage settings.</param>
    /// <param name="serverName">The MCP server name advertised to clients.</param>
    /// <param name="serverVersion">The MCP server version advertised to clients.</param>
    /// <returns>The <see cref="IMcpServerBuilder"/> for further MCP server customization.</returns>
    public static IMcpServerBuilder AddGmailMcpStdioServer(
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
            .WithStdioServerTransport()
            .WithGmailMcpTools();
    }
}
