namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic representation of an email message part (internal - used by service layer)
/// </summary>
public  sealed class EmailMessagePart
{
    public string PartId { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public IEnumerable<KeyValuePair<string, string>> Headers { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>();
    public EmailMessagePartBody? Body { get; set; }
    public IEnumerable<EmailMessagePart> Parts { get; set; } = Enumerable.Empty<EmailMessagePart>();
}

/// <summary>
/// Provider-agnostic representation of message part body (internal - used by service layer)
/// </summary>
public  sealed class EmailMessagePartBody
{
    public string AttachmentId { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Data { get; set; } = string.Empty;
}
