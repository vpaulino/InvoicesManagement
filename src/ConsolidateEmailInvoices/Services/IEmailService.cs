using Google.Apis.Gmail.v1.Data;
using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Services;

public interface IEmailService
{
    Task<IEnumerable<Message>> GetEmailsAsync(EmailQuery query);
    Task<Message> GetMessageDetailsAsync(string messageId);
    Task<MessagePartBody> GetAttachmentAsync(string messageId, string attachmentId);
    Task MarkAsReadAsync(string messageId);
}
