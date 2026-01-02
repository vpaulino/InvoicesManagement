namespace ExtractLoadInvoices.Models;

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string AttachmentId { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string MessageId { get; set; } = string.Empty;
}
