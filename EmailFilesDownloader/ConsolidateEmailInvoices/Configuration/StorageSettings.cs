using ExtractLoadInvoices.Storage;

namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Storage configuration settings
/// </summary>
public class StorageSettings
{
    public StorageType DefaultStorageType { get; set; } = StorageType.FileSystem;
    public FileSystemStorageSettings? FileSystem { get; set; }
}

/// <summary>
/// FileSystem storage specific settings
/// </summary>
public class FileSystemStorageSettings
{
    public string BaseDirectory { get; set; } = "./invoices";
    public FileNamingStrategy DefaultNamingStrategy { get; set; } = FileNamingStrategy.WithDateAndSender;
    public bool CreateVendorSubfolders { get; set; } = true;
}
