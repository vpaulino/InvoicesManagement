using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace ExtractLoadInvoices.Authentication;

public class GoogleAuthenticator : IGoogleAuthenticator
{
    public async Task<UserCredential> AuthenticateAsync(string credentialsPath, string tokenPath, string[] scopes)
    {
        if (!File.Exists(credentialsPath))
            throw new FileNotFoundException($"Credentials file not found: {credentialsPath}");

        using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
        
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(tokenPath, true));

        Console.WriteLine($"Credential file saved to: {tokenPath}");
        
        return credential;
    }
}
