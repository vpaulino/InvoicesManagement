using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.FileSystem;
using ExtractLoadInvoices.Models;
using Microsoft.Extensions.Logging;

namespace ExtractLoadInvoices.Services;

public class EmailProcessor : IEmailProcessor
{
    private readonly IEmailService _gmailService;
    private readonly IAttachmentFilter _attachmentFilter;
    private readonly IAttachmentDownloader _attachmentDownloader;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<EmailProcessor> _logger;

    public EmailProcessor(
        IEmailService gmailService,
        IAttachmentFilter attachmentFilter,
        IAttachmentDownloader attachmentDownloader,
        IFileStorageService fileStorage,
        ILogger<EmailProcessor> logger)
    {
        _gmailService = gmailService;
        _attachmentFilter = attachmentFilter;
        _attachmentDownloader = attachmentDownloader;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessEmailsFromSenderAsync(string senderEmail, string destinationFolder)
    {
        var result = new ProcessingResult();

        try
        {
            _logger.LogInformation("Processing emails from {SenderEmail}", senderEmail);

            var query = new EmailQuery
            {
                SenderEmail = senderEmail,
                UnreadOnly = true,
                IncludeSpamTrash = false,
            };

            var messages = await _gmailService.GetEmailsAsync(query);

            foreach (var message in messages)
            {
                try
                {
                    await ProcessSingleEmailAsync(message.Id, destinationFolder, result);
                    result.EmailsProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email {MessageId}", message.Id);
                    result.Errors.Add($"Email {message.Id}: {ex.Message}");
                }
            }

            _logger.LogInformation(
                "Completed processing for {SenderEmail}. Emails: {EmailCount}, Attachments: {AttachmentCount}",
                senderEmail, result.EmailsProcessed, result.AttachmentsDownloaded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing emails from {SenderEmail}", senderEmail);
            result.Errors.Add($"Sender {senderEmail}: {ex.Message}");
        }

        return result;
    }

    private async Task ProcessSingleEmailAsync(string messageId, string destinationFolder, ProcessingResult result)
    {
        var message = await _gmailService.GetMessageDetailsAsync(messageId);

        if (message.Payload?.Parts == null)
        {
            _logger.LogDebug("Email {MessageId} has no parts", messageId);
            await MarkEmailAsReadAsync(messageId);
            return;
        }

        var validAttachments = _attachmentFilter.FilterAttachments(message.Payload.Parts);

        foreach (var part in validAttachments)
        {
            try
            {
                await DownloadAndSaveAttachmentAsync(messageId, part, destinationFolder);
                result.AttachmentsDownloaded++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {FileName} from {MessageId}", 
                    part.Filename, messageId);
                result.Errors.Add($"Attachment {part.Filename}: {ex.Message}");
            }
        }

        await MarkEmailAsReadAsync(messageId);
    }

    private async Task DownloadAndSaveAttachmentAsync(string messageId, Google.Apis.Gmail.v1.Data.MessagePart part, string destinationFolder)
    {
        var attachment = await _attachmentDownloader.DownloadAttachmentAsync(
            messageId, 
            part.Body.AttachmentId, 
            part.Filename, 
            part.MimeType);

        _fileStorage.EnsureDirectoryExists(destinationFolder);
        
        var filePath = _fileStorage.GetFullPath(destinationFolder, attachment.FileName);
        
        _fileStorage.SaveFile(filePath, attachment.Data);

        _logger.LogInformation("Saved attachment: {FilePath}", filePath);
    }

    private async Task MarkEmailAsReadAsync(string messageId)
    {
        try
        {
            await _gmailService.MarkAsReadAsync(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to mark email {MessageId} as read", messageId);
        }
    }
}
