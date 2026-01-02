using Microsoft.Extensions.Configuration;

namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Configuration service implementation
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public virtual AppSettings LoadConfiguration()
    {
        var settings = new AppSettings
        {
            ApplicationName = _configuration.GetValue<string>("applicationName") ?? "InvoiceDownloader"
        };

        // Bind Google Credentials
        var googleCredentials = new Dictionary<string, string>();
        _configuration.GetSection("googleCredentials:Values").Bind(googleCredentials);
        
        settings.GoogleCredentials = new GoogleCredentialsSettings
        {
            CredentialsLocation = googleCredentials.GetValueOrDefault("credentialsLocation") ?? "credentials.json",
            TokenDestination = googleCredentials.GetValueOrDefault("tokenDestination") ?? "token.json"
        };

        // Bind Email Mappings
        var emailMappings = new Dictionary<string, string>();
        _configuration.GetSection("emailsAttachmentsDestination:Values").Bind(emailMappings);
        
        settings.EmailMappings = new EmailMappingSettings
        {
            SenderToFolderMap = emailMappings
        };

        ValidateConfiguration(settings);
        return settings;
    }

    public virtual void ValidateConfiguration(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ApplicationName))
            throw new InvalidOperationException("Application name is required.");

        if (string.IsNullOrWhiteSpace(settings.GoogleCredentials.CredentialsLocation))
            throw new InvalidOperationException("Google credentials location is required.");

        if (string.IsNullOrWhiteSpace(settings.GoogleCredentials.TokenDestination))
            throw new InvalidOperationException("Token destination is required.");

        if (settings.EmailMappings.SenderToFolderMap.Count == 0)
            throw new InvalidOperationException("At least one email-to-folder mapping is required.");
    }
}
