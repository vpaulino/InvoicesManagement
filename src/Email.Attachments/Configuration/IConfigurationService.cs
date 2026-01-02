namespace ExtractLoadInvoices.Configuration;

public interface IConfigurationService
{
    AppSettings LoadConfiguration();
    void ValidateConfiguration(AppSettings settings);
}
