using ExtractLoadInvoices.Extensions;
using ExtractLoadInvoices.Models;
using ExtractLoadInvoices.Services;

namespace ExtractLoadInvoices.Attachments;

public class AttachmentDownloader : IAttachmentDownloader
{
    private readonly IEmailService _gmailService;

    public AttachmentDownloader(IEmailService gmailService)
    {
        _gmailService = gmailService;
    }

    public async Task<EmailAttachment> DownloadAttachmentAsync(string messageId, string attachmentId, string filename, string mimeType)
    {
        var attachmentBody = await _gmailService.GetAttachmentAsync(messageId, attachmentId);
        
        var data = attachmentBody.Data.LocalDecodeBase64Url();

        return new EmailAttachment
        {
            MessageId = messageId,
            AttachmentId = attachmentId,
            FileName = filename,
            MimeType = mimeType,
            Data = data
        };
    }
}
