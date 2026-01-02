namespace ExtractLoadInvoices.Models;

/// <summary>
/// Provider-agnostic attachment data
/// </summary>
public class AttachmentData
{
    public string AttachmentId { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Data { get; set; } = string.Empty;
}
