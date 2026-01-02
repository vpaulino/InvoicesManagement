namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Configuration service interface
/// </summary>
public interface IConfigurationService
{
    AppSettings LoadConfiguration();
    void ValidateConfiguration(AppSettings settings);
}
