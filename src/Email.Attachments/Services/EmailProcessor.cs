using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.FileSystem;
using ExtractLoadInvoices.Models;
using Microsoft.Extensions.Logging;

namespace ExtractLoadInvoices.Services;

/// <summary>
/// Email processor implementation
/// </summary>
public class EmailProcessor : IEmailProcessor
{
    private readonly IEmailService _emailService;
    private readonly IAttachmentFilter _attachmentFilter;
    private readonly IAttachmentDownloader _attachmentDownloader;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<EmailProcessor> _logger;

    public EmailProcessor(
        IEmailService emailService,
        IAttachmentFilter attachmentFilter,
        IAttachmentDownloader attachmentDownloader,
        IFileStorageService fileStorage,
        ILogger<EmailProcessor> logger)
    {
        _emailService = emailService;
        _attachmentFilter = attachmentFilter;
        _attachmentDownloader = attachmentDownloader;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public virtual async Task<ProcessingResult> ProcessEmailsFromSenderAsync(string senderEmail, string destinationFolder, bool unreadOnly = true)
    {
        var result = new ProcessingResult();

        try
        {
            _logger.LogInformation("Processing emails from {SenderEmail}", senderEmail);

            var query = new GmailEmailQuery
            {
                SenderEmail = senderEmail,
                UnreadOnly = unreadOnly,
                IncludeSpamTrash = false,
            };

            var messages = await _emailService.GetEmailsAsync(query);

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

    protected virtual async Task ProcessSingleEmailAsync(string messageId, string destinationFolder, ProcessingResult result)
    {
        var message = await _emailService.GetMessageDetailsAsync(messageId);

        if (message.Payload?.Parts == null || !message.Payload.Parts.Any())
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

    protected virtual async Task DownloadAndSaveAttachmentAsync(string messageId, EmailMessagePart part, string destinationFolder)
    {
        if (part.Body == null || string.IsNullOrEmpty(part.Body.AttachmentId))
        {
            _logger.LogWarning("Part {PartId} has no attachment body", part.PartId);
            return;
        }

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

    protected virtual async Task MarkEmailAsReadAsync(string messageId)
    {
        try
        {
            await _emailService.MarkAsReadAsync(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to mark email {MessageId} as read", messageId);
        }
    }
}
