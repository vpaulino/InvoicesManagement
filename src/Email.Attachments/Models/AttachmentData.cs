namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic attachment data (internal - used by service layer)
/// </summary>
public  sealed class AttachmentData
{
    public string AttachmentId { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Data { get; set; } = string.Empty;
}
