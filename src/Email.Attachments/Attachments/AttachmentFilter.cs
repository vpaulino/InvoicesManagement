using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

/// <summary>
/// Attachment filter implementation
/// </summary>
public class AttachmentFilter : IAttachmentFilter
{
    private readonly HashSet<string> _validMimeTypes = new()
    {
        "application/pdf",
        "application/octet-stream"
    };

    public virtual bool IsValidAttachment(EmailMessagePart part)
    {
        return !string.IsNullOrEmpty(part.Filename) && 
               _validMimeTypes.Contains(part.MimeType);
    }

    public virtual IEnumerable<EmailMessagePart> FilterAttachments(IEnumerable<EmailMessagePart> parts)
    {
        return parts.Where(IsValidAttachment);
    }
}
