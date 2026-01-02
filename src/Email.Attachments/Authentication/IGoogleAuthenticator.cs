using Google.Apis.Auth.OAuth2;

namespace ExtractLoadInvoices.Authentication;

/// <summary>
/// Google authenticator interface
/// </summary>
public interface IGoogleAuthenticator
{
    Task<UserCredential> AuthenticateAsync(string credentialsPath, string tokenPath, string[] scopes);
}
