namespace EmailAttachments.McpServer.Options;

/// <summary>
/// Controls how the MCP server obtains Google credentials.
/// </summary>
public enum GmailAuthMode
{
    /// <summary>
    /// A pre-configured service account or installed-app credential stored in
    /// <c>credentials.json</c> / <c>token.json</c> on disk. The hosting application
    /// does NOT need to set up ASP.NET authentication — all Gmail calls run under
    /// the same fixed identity.
    /// </summary>
    ServiceAccount,

    /// <summary>
    /// The hosting application handles Google OAuth sign-in (e.g. via
    /// <c>AddAuthentication().AddGoogle(...)</c>) and calls
    /// <c>SaveTokens = true</c>. The MCP server reads the bearer access-token
    /// from the current <see cref="Microsoft.AspNetCore.Http.HttpContext"/> on
    /// every request so that each user connects to their own Gmail inbox.
    /// </summary>
    UserDelegated
}

/// <summary>
/// Configuration options for the Gmail MCP server.
/// </summary>
public sealed class GmailMcpServerOptions
{
    /// <summary>
    /// How the server obtains Google credentials. Defaults to
    /// <see cref="GmailAuthMode.ServiceAccount"/>.
    /// </summary>
    public GmailAuthMode AuthMode { get; set; } = GmailAuthMode.ServiceAccount;

    /// <summary>
    /// Human-readable name reported to MCP clients during the protocol handshake.
    /// </summary>
    public string ServerName { get; set; } = "Gmail MCP Server";

    /// <summary>
    /// Version string reported to MCP clients during the protocol handshake.
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    // ── ServiceAccount-only settings ─────────────────────────────────────────

    /// <summary>
    /// Path to the OAuth2 <c>credentials.json</c> file downloaded from the Google
    /// Cloud Console. Only used when <see cref="AuthMode"/> is
    /// <see cref="GmailAuthMode.ServiceAccount"/>.
    /// </summary>
    public string CredentialsPath { get; set; } = "credentials.json";

    /// <summary>
    /// Directory (or file prefix) where the OAuth2 token is cached locally.
    /// Only used when <see cref="AuthMode"/> is
    /// <see cref="GmailAuthMode.ServiceAccount"/>.
    /// </summary>
    public string TokenPath { get; set; } = "token.json";

    /// <summary>
    /// Application name sent to the Gmail API. Only used when
    /// <see cref="AuthMode"/> is <see cref="GmailAuthMode.ServiceAccount"/>.
    /// </summary>
    public string ApplicationName { get; set; } = "Gmail MCP Server";

    // ── Storage settings (both modes) ────────────────────────────────────────

    /// <summary>
    /// Base directory where downloaded attachments are persisted on disk.
    /// </summary>
    public string BaseStorageDirectory { get; set; } = "./invoices";
}
