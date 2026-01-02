namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Application settings (public for configuration binding)
/// </summary>
public sealed class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public GoogleCredentialsSettings GoogleCredentials { get; set; } = new();
    public EmailMappingSettings EmailMappings { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
}
