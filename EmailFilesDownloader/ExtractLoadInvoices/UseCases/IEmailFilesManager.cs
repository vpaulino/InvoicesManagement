
namespace ExtractLoadInvoices.UseCases
{
    public interface IEmailFilesManager
    {
        Task<EmailFilesBatch> FetchEmailFilesByPeriodAsync(TimePeriod period, FetchOptions? options = null);
        Task<EmailFilesBatch> FetchEmailFilesByVendorAsync(string vendorEmail, TimePeriod? period = null, FetchOptions? options = null);
        Task<EmailFilesBatch> FetchEmailFilesByVendorsAsync(IEnumerable<string> vendorEmails, TimePeriod? period = null, FetchOptions? options = null);
        Task<EmailFilesBatch> FetchLastMonthEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchLastNDaysEmailFilesAsync(int days, FetchOptions? options = null);
        Task<EmailFilesBatch> FetchLastQuarterEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchLastWeekEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchLastYearEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchThisMonthEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchThisQuarterEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchThisWeekEmailFilesAsync(FetchOptions? options = null);
        Task<EmailFilesBatch> FetchThisYearEmailFilesAsync(FetchOptions? options = null);
        Task<IEnumerable<VendorInfo>> GetConfiguredVendorsAsync();
    }
}