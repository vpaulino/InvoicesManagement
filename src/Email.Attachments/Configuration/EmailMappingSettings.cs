namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Email mapping settings (public for configuration binding)
/// </summary>
public sealed class EmailMappingSettings
{
    public Dictionary<string, string> SenderToFolderMap { get; set; } = new();
}
