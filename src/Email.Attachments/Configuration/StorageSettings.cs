using ExtractLoadInvoices.Storage;

namespace ExtractLoadInvoices.Configuration;

/// <summary>
/// Storage configuration settings (public for configuration binding)
/// </summary>
public sealed class StorageSettings
{
    public StorageType DefaultStorageType { get; set; } = StorageType.FileSystem;
    public FileSystemStorageSettings? FileSystem { get; set; }
}

/// <summary>
/// FileSystem storage specific settings (public for configuration binding)
/// </summary>
public sealed class FileSystemStorageSettings
{
    public string BaseDirectory { get; set; } = "./invoices";
    public FileNamingStrategy DefaultNamingStrategy { get; set; } = FileNamingStrategy.WithDateAndSender;
    public bool CreateVendorSubfolders { get; set; } = true;
}
