namespace ExtractLoadInvoices.FileSystem;

/// <summary>
/// File storage service implementation
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    public virtual void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public virtual void SaveFile(string filePath, byte[] data)
    {
        File.WriteAllBytes(filePath, data);
    }

    public virtual string GetFullPath(string directory, string fileName)
    {
        return Path.Combine(directory, fileName);
    }
}
