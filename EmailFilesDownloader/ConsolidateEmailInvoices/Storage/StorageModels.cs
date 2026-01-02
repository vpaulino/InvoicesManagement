namespace ExtractLoadInvoices.Storage;

/// <summary>
/// Result of a storage operation
/// </summary>
public class AttachmentStorageResult
{
    public bool Success { get; set; }
    public string StorageReference { get; set; } = string.Empty;
    public string? Error { get; set; }
    public long BytesWritten { get; set; }
    public StorageType StorageType { get; set; }
}

/// <summary>
/// Types of storage backends
/// </summary>
public enum StorageType
{
    FileSystem,
    AzureBlobStorage,
    AWSS3,
    DatabaseBlob,
    OneDrive,
    GoogleDrive,
    Custom
}

/// <summary>
/// Storage statistics
/// </summary>
public class StorageStatistics
{
    public long TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public Dictionary<string, long> FilesByVendor { get; set; } = new();
    public DateTime? OldestFile { get; set; }
    public DateTime? NewestFile { get; set; }
}
