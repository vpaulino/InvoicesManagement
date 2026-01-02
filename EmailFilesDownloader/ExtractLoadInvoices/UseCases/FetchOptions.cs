using ExtractLoadInvoices.Storage;

namespace ExtractLoadInvoices.UseCases;

/// <summary>
/// Options for fetching invoices with flexible configuration
/// </summary>
public class FetchOptions
{
    // What to retrieve
    public bool IncludeMetadata { get; set; } = true;
    public bool IncludeEmailBody { get; set; } = false;
    public bool IncludeAttachments { get; set; } = true;
    
    // Attachment handling strategy
    public AttachmentHandlingStrategy AttachmentStrategy { get; set; } = 
        AttachmentHandlingStrategy.PersistAndReference;
    
    // Storage configuration (optional - uses DI default if null)
    public IAttachmentPersistenceManager? PersistenceManager { get; set; }
    public string? DestinationFolder { get; set; }
    public FileNamingStrategy? NamingStrategy { get; set; }
    
    // Filtering
    public bool UnreadOnly { get; set; } = true;
    public bool MarkAsRead { get; set; } = true;
    
    // Performance
    public int? MaxResults { get; set; }
    public bool ParallelDownload { get; set; } = false;
}

/// <summary>
/// Strategy for handling attachments in memory vs storage
/// </summary>
public enum AttachmentHandlingStrategy
{
    /// <summary>
    /// Only include attachment metadata (no download, no data in memory)
    /// </summary>
    MetadataOnly,
    
    /// <summary>
    /// Load attachment data into memory (byte[] in Invoice.Attachments[].Data)
    /// WARNING: Can consume lots of memory for large files!
    /// </summary>
    LoadInMemory,
    
    /// <summary>
    /// Persist to storage and return only storage reference (RECOMMENDED)
    /// Attachments stored via IAttachmentPersistenceManager
    /// </summary>
    PersistAndReference,
    
    /// <summary>
    /// Both persist AND load in memory (for immediate processing)
    /// </summary>
    PersistAndLoad
}
