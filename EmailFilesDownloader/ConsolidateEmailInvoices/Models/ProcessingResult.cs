namespace ExtractLoadInvoices.Models;

public class ProcessingResult
{
    public int EmailsProcessed { get; set; }
    public int AttachmentsDownloaded { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => Errors.Count == 0;
}
