using Microsoft.Extensions.DependencyInjection;

namespace EmailAttachments.McpServer;

/// <summary>
/// Extension methods for registering the Gmail MCP tools with an <see cref="IMcpServerBuilder"/>.
/// </summary>
public static class GmailMcpServerBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="GmailMcpTools"/> MCP tool type with the MCP server builder.
    /// The tool type exposes all <c>IEmailFilesManager</c> use-cases as MCP tools and is
    /// host-transport-agnostic (works with both ASP.NET Core HTTP and stdio transports).
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static IMcpServerBuilder WithGmailMcpTools(this IMcpServerBuilder builder)
        => builder.WithTools<GmailMcpTools>();
}
