using ExtractLoadInvoices.Storage;

namespace ExtractLoadInvoices.UseCases;

/// <summary>
/// Represents a batch of invoices with metadata
/// </summary>
public class EmailFilesBatch
{
    public List<EmailAttachment> EmailAttachments { get; set; } = new();
    public BatchMetadata Metadata { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => Errors.Count == 0;
}

/// <summary>
/// Represents a single invoice with all available data
/// </summary>
public class EmailAttachment
{
    // Email Metadata
    public string MessageId { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    
    // Email Content (optional - based on FetchOptions)
    public string? EmailBody { get; set; }
    public string? EmailBodyPlainText { get; set; }
    
    // Attachments
    public List<FileAttachment> Attachments { get; set; } = new();
    
    // Business fields (could be extracted from email)
    public string? VendorName { get; set; }
 
}

/// <summary>
/// Attachment with flexible data handling
/// </summary>
public class FileAttachment
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string AttachmentId { get; set; } = string.Empty;
    
    // Storage information (populated if persisted)
    public bool IsPersisted { get; set; }
    public string? StorageReference { get; set; }
    public StorageType? StorageType { get; set; }
    
    // Data (only populated based on AttachmentHandlingStrategy)
    public byte[]? Data { get; set; }
    
    // Helper properties
    public bool IsInMemory => Data != null;
    public bool IsAccessible => IsPersisted || IsInMemory;
}

/// <summary>
/// Batch processing metadata
/// </summary>
public class BatchMetadata
{
    public int TotalEmails { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalAttachments { get; set; }
    public long TotalSizeBytes { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public TimePeriod? Period { get; set; }
    public Dictionary<string, int> InvoicesByVendor { get; set; } = new();
}

/// <summary>
/// Vendor information summary
/// </summary>
public class VendorInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
   
   
}
