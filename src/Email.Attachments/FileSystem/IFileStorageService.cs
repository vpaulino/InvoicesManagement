namespace ExtractLoadInvoices.FileSystem;

/// <summary>
/// File storage service interface
/// </summary>
public interface IFileStorageService
{
    void EnsureDirectoryExists(string path);
    void SaveFile(string filePath, byte[] data);
    string GetFullPath(string directory, string fileName);
}
