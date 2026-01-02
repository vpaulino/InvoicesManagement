using ExtractLoadInvoices.Extensions;
using ExtractLoadInvoices.Models;
using ExtractLoadInvoices.Services;

namespace ExtractLoadInvoices.Attachments;

public class AttachmentDownloader : IAttachmentDownloader
{
    private readonly IEmailService _emailService;

    public AttachmentDownloader(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<EmailAttachment> DownloadAttachmentAsync(string messageId, string attachmentId, string filename, string mimeType)
    {
        var attachmentData = await _emailService.GetAttachmentAsync(messageId, attachmentId);
        
        var data = attachmentData.Data.DecodeBase64Url();

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
