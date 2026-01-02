using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.Configuration;
using ExtractLoadInvoices.Models;
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Storage;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ExtractLoadInvoices.UseCases;

/// <summary>
/// High-level invoice manager with use-case oriented API
/// </summary>
public class EmailFilesManager : IEmailFilesManager
{
    private readonly IEmailService _emailService;
    private readonly IAttachmentFilter _attachmentFilter;
    private readonly IAttachmentDownloader _attachmentDownloader;
    private readonly IAttachmentPersistenceManager _defaultPersistenceManager;
    private readonly IConfigurationService _configService;
    private readonly ILogger<EmailFilesManager> _logger;

    public EmailFilesManager(
        IEmailService emailService,
        IAttachmentFilter attachmentFilter,
        IAttachmentDownloader attachmentDownloader,
        IAttachmentPersistenceManager defaultPersistenceManager,
        IConfigurationService configService,
        ILogger<EmailFilesManager> logger)
    {
        _emailService = emailService;
        _attachmentFilter = attachmentFilter;
        _attachmentDownloader = attachmentDownloader;
        _defaultPersistenceManager = defaultPersistenceManager;
        _configService = configService;
        _logger = logger;
    }

    public Task<EmailFilesBatch> FetchLastWeekEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.LastWeek(), options);

    public Task<EmailFilesBatch> FetchThisWeekEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.ThisWeek(), options);

    public Task<EmailFilesBatch> FetchThisMonthEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.ThisMonth(), options);

    public Task<EmailFilesBatch> FetchLastMonthEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.LastMonth(), options);

    public Task<EmailFilesBatch> FetchLastQuarterEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.LastQuarter(), options);

    public Task<EmailFilesBatch> FetchThisQuarterEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.ThisQuarter(), options);

    public Task<EmailFilesBatch> FetchLastYearEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.LastYear(), options);

    public Task<EmailFilesBatch> FetchThisYearEmailFilesAsync(FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.ThisYear(), options);

    public Task<EmailFilesBatch> FetchLastNDaysEmailFilesAsync(int days, FetchOptions? options = null)
        => FetchEmailFilesByPeriodAsync(TimePeriod.LastNDays(days), options);

    public async Task<EmailFilesBatch> FetchEmailFilesByPeriodAsync(TimePeriod period, FetchOptions? options = null)
    {
        var settings = _configService.LoadConfiguration();
        var vendorEmails = settings.EmailMappings.SenderToFolderMap.Keys;

        return await FetchEmailFilesByVendorsAsync(vendorEmails, period, options);
    }

    public Task<EmailFilesBatch> FetchEmailFilesByVendorAsync(
        string vendorEmail,
        TimePeriod? period = null,
        FetchOptions? options = null)
    {
        return FetchEmailFilesByVendorsAsync(new[] { vendorEmail }, period, options);
    }

    public async Task<EmailFilesBatch> FetchEmailFilesByVendorsAsync(
        IEnumerable<string> vendorEmails,
        TimePeriod? period = null,
        FetchOptions? options = null)
    {
        var sw = Stopwatch.StartNew();
        options ??= new FetchOptions();

        var batch = new EmailFilesBatch
        {
            Metadata = new BatchMetadata { Period = period }
        };

        _logger.LogInformation("Fetching invoices from {VendorCount} vendors. Period: {Period}",
            vendorEmails.Count(), period?.Description ?? "All time");

        foreach (var vendorEmail in vendorEmails)
        {
            try
            {
                var vendorInvoices = await FetchInvoicesForVendorAsync(vendorEmail, period, options);
                batch.EmailAttachments.AddRange(vendorInvoices);

                if (!batch.Metadata.InvoicesByVendor.ContainsKey(vendorEmail))
                    batch.Metadata.InvoicesByVendor[vendorEmail] = 0;

                batch.Metadata.InvoicesByVendor[vendorEmail] += vendorInvoices.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices from vendor {Vendor}", vendorEmail);
                batch.Errors.Add($"Vendor {vendorEmail}: {ex.Message}");
            }
        }

        batch.Metadata.TotalEmails = batch.EmailAttachments.Count;
        batch.Metadata.TotalInvoices = batch.EmailAttachments.Count;
        batch.Metadata.TotalAttachments = batch.EmailAttachments.Sum(i => i.Attachments.Count);
        batch.Metadata.TotalSizeBytes = batch.EmailAttachments.SelectMany(i => i.Attachments).Sum(a => a.FileSize);
        batch.Metadata.ProcessingTime = sw.Elapsed;

        _logger.LogInformation(
            "Fetched {InvoiceCount} invoices with {AttachmentCount} attachments in {Duration}ms",
            batch.Metadata.TotalInvoices,
            batch.Metadata.TotalAttachments,
            sw.ElapsedMilliseconds);

        return batch;
    }

    public Task<IEnumerable<VendorInfo>> GetConfiguredVendorsAsync()
    {
        var settings = _configService.LoadConfiguration();
        var vendors = settings.EmailMappings.SenderToFolderMap
            .Select(kvp => new VendorInfo
            {
                Email = kvp.Key,
                Name = kvp.Key.Split('@')[0]
            });

        return Task.FromResult(vendors);
    }

    private async Task<List<EmailAttachment>> FetchInvoicesForVendorAsync(
        string vendorEmail,
        TimePeriod? period,
        FetchOptions options)
    {
        var invoices = new List<EmailAttachment>();

        // Build email query
        var query = new EmailQuery
        {
            SenderEmail = vendorEmail,
            UnreadOnly = options.UnreadOnly
        };

        if (period != null)
        {
            query.After = period.StartDate;
            query.Before = period.EndDate;
        }

        // Fetch emails
        var messages = await _emailService.GetEmailsAsync(query);

        if (options.MaxResults.HasValue)
        {
            messages = messages.Take(options.MaxResults.Value);
        }

        foreach (var message in messages)
        {
            try
            {
                var invoice = await ProcessEmailToInvoiceAsync(message.Id, vendorEmail, options);
                if (invoice != null)
                {
                    invoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email {MessageId}", message.Id);
            }
        }

        return invoices;
    }

    private async Task<EmailAttachment?> ProcessEmailToInvoiceAsync(
        string messageId,
        string vendorEmail,
        FetchOptions options)
    {
        var message = await _emailService.GetMessageDetailsAsync(messageId);

        var invoice = new EmailAttachment
        {
            MessageId = messageId,
            Sender = vendorEmail,
            SenderName = ExtractSenderName(message),
            VendorName = vendorEmail.Split('@')[0]
        };

        // Extract metadata
        if (options.IncludeMetadata && message.Payload?.Headers != null)
        {
            invoice.Subject = GetHeaderValue(message.Payload.Headers, "Subject") ?? "";
            var dateStr = GetHeaderValue(message.Payload.Headers, "Date");
            if (DateTime.TryParse(dateStr, out var sentDate))
            {
                invoice.SentDate = sentDate;
            }
        }

        // Extract email body
        if (options.IncludeEmailBody && message.Payload != null)
        {
            invoice.EmailBody = ExtractEmailBody(message.Payload);
            invoice.EmailBodyPlainText = ExtractPlainTextBody(message.Payload);
        }

        // Process attachments
        if (options.IncludeAttachments && message.Payload?.Parts != null)
        {
            var validParts = _attachmentFilter.FilterAttachments(message.Payload.Parts);

            foreach (var part in validParts)
            {
                var attachment = await ProcessAttachmentAsync(
                    messageId,
                    part,
                    vendorEmail,
                    invoice.SentDate,
                    options);

                if (attachment != null)
                {
                    invoice.Attachments.Add(attachment);
                }
            }
        }

        // Mark as read if requested
        if (options.MarkAsRead)
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

        return invoice;
    }

    private async Task<FileAttachment?> ProcessAttachmentAsync(
        string messageId,
        Google.Apis.Gmail.v1.Data.MessagePart part,
        string vendorEmail,
        DateTime emailDate,
        FetchOptions options)
    {
        var attachment = new FileAttachment
        {
            FileName = part.Filename,
            MimeType = part.MimeType,
            AttachmentId = part.Body?.AttachmentId ?? "",
            FileSize = part.Body?.Size ?? 0
        };

        var persistenceManager = options.PersistenceManager ?? _defaultPersistenceManager;

        switch (options.AttachmentStrategy)
        {
            case AttachmentHandlingStrategy.MetadataOnly:
                // Only metadata, no download
                break;

            case AttachmentHandlingStrategy.LoadInMemory:
                // Download and keep in memory
                var downloaded = await _attachmentDownloader.DownloadAttachmentAsync(
                    messageId, part.Body?.AttachmentId ?? "", part.Filename, part.MimeType);
                attachment.Data = downloaded.Data;
                break;

            case AttachmentHandlingStrategy.PersistAndReference:
                // Download and persist, don't keep in memory
                var downloadedForPersist = await _attachmentDownloader.DownloadAttachmentAsync(
                    messageId, part.Body?.AttachmentId ?? "", part.Filename, part.MimeType);

                var context = CreateAttachmentContext(vendorEmail, emailDate, downloadedForPersist, options);
                var result = await persistenceManager.SaveAttachmentAsync(context, downloadedForPersist.Data);

                if (result.Success)
                {
                    attachment.IsPersisted = true;
                    attachment.StorageReference = result.StorageReference;
                    attachment.StorageType = result.StorageType;
                }
                break;

            case AttachmentHandlingStrategy.PersistAndLoad:
                // Download, persist, AND keep in memory
                var downloadedForBoth = await _attachmentDownloader.DownloadAttachmentAsync(
                    messageId, part.Body?.AttachmentId ?? "", part.Filename, part.MimeType);

                var contextBoth = CreateAttachmentContext(vendorEmail, emailDate, downloadedForBoth, options);
                var resultBoth = await persistenceManager.SaveAttachmentAsync(contextBoth, downloadedForBoth.Data);

                if (resultBoth.Success)
                {
                    attachment.IsPersisted = true;
                    attachment.StorageReference = resultBoth.StorageReference;
                    attachment.StorageType = resultBoth.StorageType;
                }
                attachment.Data = downloadedForBoth.Data;
                break;
        }

        return attachment;
    }

    private AttachmentContext CreateAttachmentContext(
        string vendorEmail,
        DateTime emailDate,
        Models.EmailAttachment downloaded,
        FetchOptions options)
    {
        return new AttachmentContext
        {
            FileName = downloaded.FileName,
            VendorEmail = vendorEmail,
            VendorName = vendorEmail.Split('@')[0],
            EmailDate = emailDate,
            MimeType = downloaded.MimeType,
            MessageId = downloaded.MessageId,
            FileSize = downloaded.Data?.Length ?? 0,
            SuggestedFolder = options.DestinationFolder,
            NamingStrategy = options.NamingStrategy
        };
    }

    private string ExtractSenderName(Google.Apis.Gmail.v1.Data.Message message)
    {
        var from = GetHeaderValue(message.Payload?.Headers, "From");
        if (string.IsNullOrEmpty(from))
            return "";

        // Extract name from "Name <email@domain.com>"
        var match = System.Text.RegularExpressions.Regex.Match(from, @"^(.+?)\s*<");
        return match.Success ? match.Groups[1].Value.Trim() : from;
    }

    private string? GetHeaderValue(IList<Google.Apis.Gmail.v1.Data.MessagePartHeader>? headers, string name)
    {
        return headers?.FirstOrDefault(h =>
            h.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)?.Value;
    }

    private string? ExtractEmailBody(Google.Apis.Gmail.v1.Data.MessagePart payload)
    {
        // Try to get HTML body
        if (!string.IsNullOrEmpty(payload.Body?.Data))
        {
            return DecodeBase64(payload.Body.Data);
        }

        // Recursively search parts
        if (payload.Parts != null)
        {
            foreach (var part in payload.Parts)
            {
                if (part.MimeType == "text/html" && !string.IsNullOrEmpty(part.Body?.Data))
                {
                    return DecodeBase64(part.Body.Data);
                }
            }
        }

        return null;
    }

    private string? ExtractPlainTextBody(Google.Apis.Gmail.v1.Data.MessagePart payload)
    {
        if (!string.IsNullOrEmpty(payload.Body?.Data))
        {
            return DecodeBase64(payload.Body.Data);
        }

        if (payload.Parts != null)
        {
            foreach (var part in payload.Parts)
            {
                if (part.MimeType == "text/plain" && !string.IsNullOrEmpty(part.Body?.Data))
                {
                    return DecodeBase64(part.Body.Data);
                }
            }
        }

        return null;
    }

    private string DecodeBase64(string base64String)
    {
        try
        {
            var data = Convert.FromBase64String(base64String.Replace('-', '+').Replace('_', '/'));
            return System.Text.Encoding.UTF8.GetString(data);
        }
        catch
        {
            return base64String;
        }
    }
}
