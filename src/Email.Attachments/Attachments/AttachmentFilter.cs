using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

public class AttachmentFilter : IAttachmentFilter
{
    private readonly HashSet<string> _validMimeTypes = new()
    {
        "application/pdf",
        "application/octet-stream"
    };

    public bool IsValidAttachment(EmailMessagePart part)
    {
        return !string.IsNullOrEmpty(part.Filename) && 
               _validMimeTypes.Contains(part.MimeType);
    }

    public IEnumerable<EmailMessagePart> FilterAttachments(IEnumerable<EmailMessagePart> parts)
    {
        return parts.Where(IsValidAttachment);
    }
}
