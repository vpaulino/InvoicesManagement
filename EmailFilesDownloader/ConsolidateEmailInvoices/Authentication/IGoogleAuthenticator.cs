using Google.Apis.Auth.OAuth2;

namespace ExtractLoadInvoices.Authentication;

public interface IGoogleAuthenticator
{
    Task<UserCredential> AuthenticateAsync(string credentialsPath, string tokenPath, string[] scopes);
}
