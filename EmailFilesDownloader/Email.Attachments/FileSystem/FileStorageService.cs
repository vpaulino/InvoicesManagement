namespace ExtractLoadInvoices.FileSystem;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;

    public FileStorageService()
    {
        _baseDirectory = Environment.CurrentDirectory;
    }

    public void EnsureDirectoryExists(string path)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public void SaveFile(string filePath, byte[] data)
    {
        File.WriteAllBytes(filePath, data);
    }

    public string GetFullPath(string directory, string fileName)
    {
        var fullDirectory = Path.Combine(_baseDirectory, directory);
        return Path.Combine(fullDirectory, fileName);
    }
}
