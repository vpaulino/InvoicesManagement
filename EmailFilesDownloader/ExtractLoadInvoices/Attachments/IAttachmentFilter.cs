using Google.Apis.Gmail.v1.Data;

namespace ExtractLoadInvoices.Attachments;

public interface IAttachmentFilter
{
    bool IsValidAttachment(MessagePart part);
    IEnumerable<MessagePart> FilterAttachments(IEnumerable<MessagePart> parts);
}
