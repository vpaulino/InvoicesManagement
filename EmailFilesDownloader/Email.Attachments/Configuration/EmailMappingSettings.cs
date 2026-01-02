namespace ExtractLoadInvoices.Configuration;

public class EmailMappingSettings
{
    public Dictionary<string, string> SenderToFolderMap { get; set; } = new();
}
