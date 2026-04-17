using Google.Apis.Auth.OAuth2;

namespace EmailAttachments.McpServer.Authentication;

/// <summary>
/// Provides a Google <see cref="ICredential"/> that the MCP server uses to
/// authenticate Gmail API calls. The implementation is swapped at runtime
/// depending on the configured <see cref="Options.GmailAuthMode"/>.
/// </summary>
public interface IGmailCredentialProvider
{
    /// <summary>
    /// Returns a ready-to-use Google credential. For
    /// <see cref="Options.GmailAuthMode.UserDelegated"/> the credential is
    /// obtained from the current HTTP request's authentication token; for
    /// <see cref="Options.GmailAuthMode.ServiceAccount"/> it is loaded from
    /// disk once at startup.
    /// </summary>
    Task<ICredential> GetCredentialAsync(CancellationToken cancellationToken = default);
}
