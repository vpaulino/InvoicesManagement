namespace ExtractLoadInvoices.Storage;

/// <summary>
/// Context information for storing an attachment
/// </summary>
public class AttachmentContext
{
    public string FileName { get; set; } = string.Empty;
    public string VendorEmail { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateTime EmailDate { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    // Configuration hints
    public string? SuggestedFolder { get; set; }
    public FileNamingStrategy? NamingStrategy { get; set; }
}

/// <summary>
/// File naming strategies
/// </summary>
public enum FileNamingStrategy
{
    /// <summary>
    /// Keep original filename
    /// </summary>
    Original,
    
    /// <summary>
    /// Add timestamp: filename_20241215_143022.pdf
    /// </summary>
    WithTimestamp,
    
    /// <summary>
    /// Add sender: vendorname_invoice.pdf
    /// </summary>
    WithSender,
    
    /// <summary>
    /// Add date and sender: 20241215_vendorname_invoice.pdf
    /// </summary>
    WithDateAndSender
}
