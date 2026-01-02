using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Services;

/// <summary>
/// Email processor interface
/// </summary>
public interface IEmailProcessor
{
    Task<ProcessingResult> ProcessEmailsFromSenderAsync(string senderEmail, string destinationFolder, bool unreadOnly = true);
}
