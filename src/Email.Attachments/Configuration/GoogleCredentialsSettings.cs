namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Google credentials settings (public for configuration binding)
/// </summary>
public sealed class GoogleCredentialsSettings
{
    public string CredentialsLocation { get; set; } = string.Empty;
    public string TokenDestination { get; set; } = string.Empty;
}
