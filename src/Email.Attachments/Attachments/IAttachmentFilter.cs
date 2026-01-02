using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

/// <summary>
/// Attachment filter interface
/// </summary>
public interface IAttachmentFilter
{
    bool IsValidAttachment(EmailMessagePart part);
    IEnumerable<EmailMessagePart> FilterAttachments(IEnumerable<EmailMessagePart> parts);
}
