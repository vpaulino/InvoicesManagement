namespace ExtractLoadInvoices.Configuration;

public class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public GoogleCredentialsSettings GoogleCredentials { get; set; } = new();
    public EmailMappingSettings EmailMappings { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
}
