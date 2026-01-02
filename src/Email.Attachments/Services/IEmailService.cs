using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Services;

/// <summary>
/// Provider-agnostic email service interface
/// </summary>
public interface IEmailService
{
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailQuery query);
    Task<EmailMessageDetails> GetMessageDetailsAsync(string messageId);
    Task<AttachmentData> GetAttachmentAsync(string messageId, string attachmentId);
    Task MarkAsReadAsync(string messageId);
}
