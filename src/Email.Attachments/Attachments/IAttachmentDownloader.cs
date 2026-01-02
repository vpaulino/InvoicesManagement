using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Attachments;

/// <summary>
/// Attachment downloader interface
/// </summary>
public interface IAttachmentDownloader
{
    Task<EmailAttachment> DownloadAttachmentAsync(string messageId, string attachmentId, string filename, string mimeType);
}
