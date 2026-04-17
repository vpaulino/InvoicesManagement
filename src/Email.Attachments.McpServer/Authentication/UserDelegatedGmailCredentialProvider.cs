using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace EmailAttachments.McpServer.Authentication;

/// <summary>
/// Provides a Google credential sourced from the currently signed-in user's
/// OAuth2 access token. The hosting application must configure Google
/// authentication with <c>SaveTokens = true</c> so that the token is available
/// in the request cookie.
/// </summary>
/// <remarks>
/// Typical hosting-app setup:
/// <code>
/// builder.Services
///     .AddAuthentication(options => { ... })
///     .AddGoogle(options =>
///     {
///         options.ClientId     = "...";
///         options.ClientSecret = "...";
///         options.SaveTokens   = true;
///         options.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");
///         options.Scope.Add("https://www.googleapis.com/auth/gmail.modify");
///     });
/// </code>
/// </remarks>
public sealed class UserDelegatedGmailCredentialProvider : IGmailCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialises the provider with the HTTP context accessor used to read the
    /// bearer token from the current request.
    /// </summary>
    public UserDelegatedGmailCredentialProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<ICredential> GetCredentialAsync(
        CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "UserDelegatedGmailCredentialProvider requires an active HTTP request context.");

        var accessToken = await context.GetTokenAsync("access_token")
            ?? throw new InvalidOperationException(
                "No Google access_token found in the current HTTP context. " +
                "Ensure the user is authenticated and the hosting app calls " +
                "AddAuthentication().AddGoogle(...) with SaveTokens = true.");

        return GoogleCredential.FromAccessToken(accessToken);
    }
}
