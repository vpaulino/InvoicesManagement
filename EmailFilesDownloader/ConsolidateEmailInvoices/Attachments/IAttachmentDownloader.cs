using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

public interface IAttachmentDownloader
{
    Task<EmailAttachment> DownloadAttachmentAsync(string messageId, string attachmentId, string filename, string mimeType);
}
