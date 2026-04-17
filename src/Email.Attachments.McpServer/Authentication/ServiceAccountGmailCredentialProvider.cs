using EmailAttachments.McpServer.Options;
using ExtractLoadInvoices.Authentication;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace EmailAttachments.McpServer.Authentication;

/// <summary>
/// Provides a Google credential by reading <c>credentials.json</c> and caching
/// the resulting token to disk. This is the same flow used by the
/// <c>Email.Attachments</c> package and is appropriate for server-side /
/// daemon scenarios where a single Gmail identity is shared.
/// </summary>
public sealed class ServiceAccountGmailCredentialProvider : IGmailCredentialProvider
{
    private static readonly string[] Scopes =
    [
        Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly,
        Google.Apis.Gmail.v1.GmailService.Scope.GmailLabels,
        Google.Apis.Gmail.v1.GmailService.Scope.GmailModify
    ];

    private readonly IGoogleAuthenticator _authenticator;
    private readonly GmailMcpServerOptions _options;

    // Cache the credential so we only call the broker once.
    private ICredential? _cached;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initialises the provider with a Google authenticator and the server options.
    /// </summary>
    public ServiceAccountGmailCredentialProvider(
        IGoogleAuthenticator authenticator,
        IOptions<GmailMcpServerOptions> options)
    {
        _authenticator = authenticator;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ICredential> GetCredentialAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
            return _cached;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cached ??= await _authenticator.AuthenticateAsync(
                _options.CredentialsPath,
                _options.TokenPath,
                Scopes);

            return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }
}
