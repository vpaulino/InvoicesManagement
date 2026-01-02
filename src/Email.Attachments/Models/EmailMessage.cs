namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic representation of an email message (internal - used by service layer)
/// </summary>
public  sealed class EmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public IEnumerable<string> LabelIds { get; set; } = Enumerable.Empty<string>();
    public string Snippet { get; set; } = string.Empty;
    public long InternalDate { get; set; }
}
