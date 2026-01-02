namespace ExtractLoadInvoices.FileSystem;

public interface IFileStorageService
{
    void EnsureDirectoryExists(string path);
    void SaveFile(string filePath, byte[] data);
    string GetFullPath(string directory, string fileName);
}
