namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic email address representation
/// </summary>
public class EmailAddress
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
