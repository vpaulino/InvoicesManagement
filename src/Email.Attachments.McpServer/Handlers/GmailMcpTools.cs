using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmailAttachments.McpServer.Authentication;
using ExtractLoadInvoices.Storage;
using ExtractLoadInvoices.UseCases;
using Google.Apis.Services;
using ModelContextProtocol.Server;
using GmailApi = Google.Apis.Gmail.v1;

namespace EmailAttachments.McpServer.Handlers;

/// <summary>
/// All Gmail MCP tools exposed by this package. Each public method decorated with
/// <see cref="McpServerToolAttribute"/> becomes a callable tool that an AI
/// assistant (Claude, Copilot, etc.) can invoke.
/// </summary>
[McpServerToolType]
public sealed class GmailMcpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IEmailFilesManager _emailFilesManager;
    private readonly IAttachmentPersistenceManager _persistenceManager;
    private readonly IGmailCredentialProvider _credentialProvider;

    /// <summary>
    /// Initialises the tool class with required services from the DI container.
    /// </summary>
    public GmailMcpTools(
        IEmailFilesManager emailFilesManager,
        IAttachmentPersistenceManager persistenceManager,
        IGmailCredentialProvider credentialProvider)
    {
        _emailFilesManager = emailFilesManager;
        _persistenceManager = persistenceManager;
        _credentialProvider = credentialProvider;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // list_vendors
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all configured vendor email-to-folder mappings.
    /// </summary>
    [McpServerTool(Name = "list_vendors")]
    [Description("List all vendor email addresses that are configured with a local folder mapping.")]
    public async Task<string> ListVendorsAsync()
    {
        var vendors = await _emailFilesManager.GetConfiguredVendorsAsync();
        return Serialize(vendors.Select(v => new { v.Email, v.Name }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // search_emails
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Searches Gmail for emails matching the given criteria and returns
    /// metadata (no attachment data).
    /// </summary>
    [McpServerTool(Name = "search_emails", ReadOnly = true)]
    [Description(
        "Search the Gmail inbox for emails matching the given criteria. " +
        "Returns email metadata including sender, subject, and attachment names. " +
        "No attachment data is downloaded.")]
    public async Task<string> SearchEmailsAsync(
        [Description("Filter by sender email address. Leave empty to search all senders.")]
        string? senderEmail = null,
        [Description("Only return emails received after this date (inclusive). Format: yyyy-MM-dd")]
        string? after = null,
        [Description("Only return emails received before this date (exclusive). Format: yyyy-MM-dd")]
        string? before = null,
        [Description("Maximum number of results to return. Defaults to 50.")]
        int maxResults = 50)
    {
        var period = BuildPeriod(after, before);

        EmailFilesBatch batch;
        if (!string.IsNullOrWhiteSpace(senderEmail))
        {
            batch = await _emailFilesManager.FetchEmailFilesByVendorAsync(
                senderEmail,
                period,
                new FetchOptions
                {
                    IncludeMetadata = true,
                    IncludeAttachments = true,
                    AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly,
                    UnreadOnly = false,
                    MarkAsRead = false,
                    MaxResults = maxResults
                });
        }
        else
        {
            period ??= TimePeriod.LastMonth();
            batch = await _emailFilesManager.FetchEmailFilesByPeriodAsync(
                period,
                new FetchOptions
                {
                    IncludeMetadata = true,
                    IncludeAttachments = true,
                    AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly,
                    UnreadOnly = false,
                    MarkAsRead = false,
                    MaxResults = maxResults
                });
        }

        return SerializeBatchSummary(batch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // fetch_attachments_last_week
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Download last calendar week's email attachments.</summary>
    [McpServerTool(Name = "fetch_attachments_last_week")]
    [Description(
        "Download email attachments received during the last calendar week " +
        "and persist them to the configured local storage directory.")]
    public async Task<string> FetchAttachmentsLastWeekAsync(
        [Description("When true (default) attachments are saved to disk. When false only metadata is returned.")]
        bool persistFiles = true)
    {
        var batch = await _emailFilesManager.FetchLastWeekEmailFilesAsync(
            BuildFetchOptions(persistFiles));

        return SerializeBatchSummary(batch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // fetch_attachments_last_month
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Download last calendar month's email attachments.</summary>
    [McpServerTool(Name = "fetch_attachments_last_month")]
    [Description(
        "Download email attachments received during the last calendar month " +
        "and persist them to the configured local storage directory.")]
    public async Task<string> FetchAttachmentsLastMonthAsync(
        [Description("When true (default) attachments are saved to disk. When false only metadata is returned.")]
        bool persistFiles = true)
    {
        var batch = await _emailFilesManager.FetchLastMonthEmailFilesAsync(
            BuildFetchOptions(persistFiles));

        return SerializeBatchSummary(batch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // fetch_attachments_by_period
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Download email attachments within a custom date range.</summary>
    [McpServerTool(Name = "fetch_attachments_by_period")]
    [Description(
        "Download email attachments received within a custom date range " +
        "and persist them to the configured local storage directory.")]
    public async Task<string> FetchAttachmentsByPeriodAsync(
        [Description("Start of the date range (inclusive). Format: yyyy-MM-dd")]
        string startDate,
        [Description("End of the date range (exclusive). Format: yyyy-MM-dd")]
        string endDate,
        [Description("When true (default) attachments are saved to disk. When false only metadata is returned.")]
        bool persistFiles = true)
    {
        if (!DateTime.TryParse(startDate, out var start))
            return Error($"Invalid startDate '{startDate}'. Expected format: yyyy-MM-dd");
        if (!DateTime.TryParse(endDate, out var end))
            return Error($"Invalid endDate '{endDate}'. Expected format: yyyy-MM-dd");
        if (end <= start)
            return Error("endDate must be after startDate.");

        var batch = await _emailFilesManager.FetchEmailFilesByPeriodAsync(
            TimePeriod.Custom(start, end),
            BuildFetchOptions(persistFiles));

        return SerializeBatchSummary(batch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // fetch_attachments_by_vendor
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Download email attachments from a specific sender (vendor).</summary>
    [McpServerTool(Name = "fetch_attachments_by_vendor")]
    [Description(
        "Download email attachments from a specific sender (vendor) and " +
        "persist them to the configured local storage directory.")]
    public async Task<string> FetchAttachmentsByVendorAsync(
        [Description("The sender's email address, e.g. invoices@vendor.com")]
        string vendorEmail,
        [Description("Optional start date (inclusive). Format: yyyy-MM-dd. Defaults to last 30 days.")]
        string? after = null,
        [Description("Optional end date (exclusive). Format: yyyy-MM-dd.")]
        string? before = null,
        [Description("When true (default) attachments are saved to disk. When false only metadata is returned.")]
        bool persistFiles = true)
    {
        if (string.IsNullOrWhiteSpace(vendorEmail))
            return Error("vendorEmail is required.");

        var period = BuildPeriod(after, before);

        var batch = await _emailFilesManager.FetchEmailFilesByVendorAsync(
            vendorEmail,
            period,
            BuildFetchOptions(persistFiles));

        return SerializeBatchSummary(batch);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // get_storage_stats
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Return statistics on attachments already persisted to local storage.</summary>
    [McpServerTool(Name = "get_storage_stats", ReadOnly = true)]
    [Description(
        "Return statistics about attachments already persisted to local storage: " +
        "total file count, total size, per-vendor breakdown, and date range.")]
    public async Task<string> GetStorageStatsAsync()
    {
        var stats = await _persistenceManager.GetStatisticsAsync();

        var result = new
        {
            stats.TotalFiles,
            TotalSizeMb = Math.Round(stats.TotalSizeBytes / 1024.0 / 1024.0, 2),
            OldestFile = stats.OldestFile?.ToString("yyyy-MM-dd"),
            NewestFile = stats.NewestFile?.ToString("yyyy-MM-dd"),
            ByVendor = stats.FilesByVendor
        };

        return Serialize(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static FetchOptions BuildFetchOptions(bool persistFiles) => new()
    {
        IncludeMetadata = true,
        IncludeAttachments = true,
        AttachmentStrategy = persistFiles
            ? AttachmentHandlingStrategy.PersistAndReference
            : AttachmentHandlingStrategy.MetadataOnly,
        NamingStrategy = FileNamingStrategy.WithDateAndSender,
        UnreadOnly = false
    };

    private static TimePeriod? BuildPeriod(string? after, string? before)
    {
        if (after is null && before is null)
            return null;

        var start = after is not null && DateTime.TryParse(after, out var s)
            ? s
            : DateTime.Today.AddDays(-30);

        var end = before is not null && DateTime.TryParse(before, out var e)
            ? e
            : DateTime.Today.AddDays(1);

        return TimePeriod.Custom(start, end);
    }

    private static string SerializeBatchSummary(EmailFilesBatch batch)
    {
        var summary = new
        {
            IsSuccess = batch.IsSuccess,
            TotalEmails = batch.Metadata.TotalEmails,
            TotalInvoices = batch.Metadata.TotalInvoices,
            TotalAttachments = batch.Metadata.TotalAttachments,
            TotalSizeMb = Math.Round(batch.Metadata.TotalSizeBytes / 1024.0 / 1024.0, 2),
            Period = batch.Metadata.Period?.Description,
            ProcessingTimeSeconds = Math.Round(batch.Metadata.ProcessingTime.TotalSeconds, 2),
            ByVendor = batch.Metadata.InvoicesByVendor,
            Emails = batch.EmailAttachments.Select(e => new
            {
                e.MessageId,
                SentDate = e.SentDate.ToString("yyyy-MM-dd"),
                e.Sender,
                e.SenderName,
                e.Subject,
                Attachments = e.Attachments.Select(a => new
                {
                    a.FileName,
                    SizeKb = Math.Round(a.FileSize / 1024.0, 1),
                    a.MimeType,
                    a.IsPersisted,
                    a.StorageReference
                })
            }),
            Errors = batch.Errors
        };

        return Serialize(summary);
    }

    private static string Serialize(object value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static string Error(string message) =>
        JsonSerializer.Serialize(new { Error = message }, JsonOptions);
}
