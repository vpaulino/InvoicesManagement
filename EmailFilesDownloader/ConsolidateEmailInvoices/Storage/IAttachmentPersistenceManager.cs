namespace ExtractLoadInvoices.Storage;

/// <summary>
/// Abstraction for persisting invoice attachments
/// Allows multiple storage strategies: FileSystem, Azure Blob, AWS S3, Database, etc.
/// </summary>
public interface IAttachmentPersistenceManager
{
    /// <summary>
    /// Save an attachment and return its storage location reference
    /// </summary>
    /// <param name="context">Metadata about the attachment being saved</param>
    /// <param name="data">Attachment binary data</param>
    /// <returns>Storage location identifier (file path, blob URL, etc.)</returns>
    Task<AttachmentStorageResult> SaveAttachmentAsync(
        AttachmentContext context, 
        byte[] data);
    
    /// <summary>
    /// Save an attachment from a stream (memory efficient for large files)
    /// </summary>
    Task<AttachmentStorageResult> SaveAttachmentStreamAsync(
        AttachmentContext context, 
        Stream dataStream);
    
    /// <summary>
    /// Retrieve an attachment by its storage reference
    /// </summary>
    Task<byte[]> GetAttachmentAsync(string storageReference);
    
    /// <summary>
    /// Retrieve an attachment as a stream (memory efficient)
    /// </summary>
    Task<Stream> GetAttachmentStreamAsync(string storageReference);
    
    /// <summary>
    /// Check if an attachment exists
    /// </summary>
    Task<bool> ExistsAsync(string storageReference);
    
    /// <summary>
    /// Delete an attachment
    /// </summary>
    Task<bool> DeleteAttachmentAsync(string storageReference);
    
    /// <summary>
    /// Get storage statistics
    /// </summary>
    Task<StorageStatistics> GetStatisticsAsync();
}
