using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

public interface IAttachmentFilter
{
    bool IsValidAttachment(EmailMessagePart part);
    IEnumerable<EmailMessagePart> FilterAttachments(IEnumerable<EmailMessagePart> parts);
}
