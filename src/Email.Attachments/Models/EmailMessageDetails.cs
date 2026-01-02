namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic detailed email message with full content (internal - used by service layer)
/// </summary>
public  sealed class EmailMessageDetails
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public IEnumerable<string> LabelIds { get; set; } = Enumerable.Empty<string>();
    public string Snippet { get; set; } = string.Empty;
    public long InternalDate { get; set; }
    public long SizeEstimate { get; set; }
    public ulong HistoryId { get; set; }
    public EmailMessagePart? Payload { get; set; }
    public string Raw { get; set; } = string.Empty;
    
    /// <summary>
    /// Parsed headers for common fields
    /// </summary>
    public EmailAddress? From { get; set; }
    public IEnumerable<EmailAddress> To { get; set; } = Enumerable.Empty<EmailAddress>();
    public string Subject { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
