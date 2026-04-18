using System.ComponentModel;
using ExtractLoadInvoices.UseCases;
using ModelContextProtocol.Server;

namespace EmailAttachments.McpServer;

/// <summary>
/// MCP tool definitions for Gmail email attachment management.
/// This class is host-agnostic and can be used with any MCP transport
/// (ASP.NET Core HTTP, stdio, etc.).
/// </summary>
[McpServerToolType]
public sealed class GmailMcpTools
{
    private readonly IEmailFilesManager _emailFilesManager;

    public GmailMcpTools(IEmailFilesManager emailFilesManager)
    {
        _emailFilesManager = emailFilesManager;
    }

    [McpServerTool(Name = "get_configured_vendors")]
    [Description("Returns the list of vendor email addresses configured for attachment extraction.")]
    public async Task<IEnumerable<VendorInfo>> GetConfiguredVendorsAsync()
        => await _emailFilesManager.GetConfiguredVendorsAsync();

    [McpServerTool(Name = "fetch_email_files_this_week")]
    [Description("Fetches email attachments received during the current calendar week (Monday–today).")]
    public async Task<EmailFilesBatch> FetchThisWeekEmailFilesAsync()
        => await _emailFilesManager.FetchThisWeekEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_last_week")]
    [Description("Fetches email attachments received during the previous calendar week (Monday–Sunday).")]
    public async Task<EmailFilesBatch> FetchLastWeekEmailFilesAsync()
        => await _emailFilesManager.FetchLastWeekEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_this_month")]
    [Description("Fetches email attachments received during the current calendar month.")]
    public async Task<EmailFilesBatch> FetchThisMonthEmailFilesAsync()
        => await _emailFilesManager.FetchThisMonthEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_last_month")]
    [Description("Fetches email attachments received during the previous calendar month.")]
    public async Task<EmailFilesBatch> FetchLastMonthEmailFilesAsync()
        => await _emailFilesManager.FetchLastMonthEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_this_quarter")]
    [Description("Fetches email attachments received during the current calendar quarter.")]
    public async Task<EmailFilesBatch> FetchThisQuarterEmailFilesAsync()
        => await _emailFilesManager.FetchThisQuarterEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_last_quarter")]
    [Description("Fetches email attachments received during the previous calendar quarter.")]
    public async Task<EmailFilesBatch> FetchLastQuarterEmailFilesAsync()
        => await _emailFilesManager.FetchLastQuarterEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_this_year")]
    [Description("Fetches email attachments received during the current calendar year.")]
    public async Task<EmailFilesBatch> FetchThisYearEmailFilesAsync()
        => await _emailFilesManager.FetchThisYearEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_last_year")]
    [Description("Fetches email attachments received during the previous calendar year.")]
    public async Task<EmailFilesBatch> FetchLastYearEmailFilesAsync()
        => await _emailFilesManager.FetchLastYearEmailFilesAsync();

    [McpServerTool(Name = "fetch_email_files_last_n_days")]
    [Description("Fetches email attachments received during the last N days.")]
    public async Task<EmailFilesBatch> FetchLastNDaysEmailFilesAsync(
        [Description("Number of days to look back (e.g. 7, 30, 90).")] int days)
        => await _emailFilesManager.FetchLastNDaysEmailFilesAsync(days);

    [McpServerTool(Name = "fetch_email_files_by_vendor")]
    [Description("Fetches email attachments sent by a specific vendor email address.")]
    public async Task<EmailFilesBatch> FetchEmailFilesByVendorAsync(
        [Description("The vendor's email address (e.g. invoices@supplier.com).")] string vendorEmail)
        => await _emailFilesManager.FetchEmailFilesByVendorAsync(vendorEmail);

    [McpServerTool(Name = "fetch_email_files_by_period")]
    [Description("Fetches email attachments received within a custom date range.")]
    public async Task<EmailFilesBatch> FetchEmailFilesByPeriodAsync(
        [Description("Start date in yyyy-MM-dd format (inclusive).")] string startDate,
        [Description("End date in yyyy-MM-dd format (exclusive).")] string endDate)
    {
        var period = TimePeriod.Custom(
            DateTime.Parse(startDate, System.Globalization.CultureInfo.InvariantCulture),
            DateTime.Parse(endDate, System.Globalization.CultureInfo.InvariantCulture));

        return await _emailFilesManager.FetchEmailFilesByPeriodAsync(period);
    }
}
