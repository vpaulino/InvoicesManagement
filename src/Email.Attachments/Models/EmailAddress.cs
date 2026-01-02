namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic email address representation (internal - used by service layer)
/// </summary>
public  sealed class EmailAddress
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
