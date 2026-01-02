using Google.Apis.Gmail.v1.Data;

namespace ExtractLoadInvoices.Attachments;

public class AttachmentFilter : IAttachmentFilter
{
    private readonly HashSet<string> _validMimeTypes = new()
    {
        "application/pdf",
        "application/octet-stream"
    };

    public bool IsValidAttachment(MessagePart part)
    {
        return !string.IsNullOrEmpty(part.Filename) && 
               _validMimeTypes.Contains(part.MimeType);
    }

    public IEnumerable<MessagePart> FilterAttachments(IEnumerable<MessagePart> parts)
    {
        return parts.Where(IsValidAttachment);
    }
}
