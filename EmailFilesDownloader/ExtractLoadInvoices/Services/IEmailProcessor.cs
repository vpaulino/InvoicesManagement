using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Services;

public interface IEmailProcessor
{
    Task<ProcessingResult> ProcessEmailsFromSenderAsync(string senderEmail, string destinationFolder);
}
