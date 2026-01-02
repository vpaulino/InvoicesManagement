using ExtractLoadInvoices.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExtractLoadInvoices.Storage;

/// <summary>
/// FileSystem-based attachment persistence using email-to-folder mappings
/// </summary>
public class FileSystemAttachmentPersistenceManager : IAttachmentPersistenceManager
{
    private readonly string _baseDirectory;
    private readonly EmailMappingSettings _emailMappings;
    private readonly FileNamingStrategy _defaultNamingStrategy;
    private readonly ILogger<FileSystemAttachmentPersistenceManager> _logger;

    public FileSystemAttachmentPersistenceManager(
        IOptions<AppSettings> settings,
        ILogger<FileSystemAttachmentPersistenceManager> logger)
    {
        _baseDirectory = settings.Value.Storage?.FileSystem?.BaseDirectory ?? Environment.CurrentDirectory;
        _emailMappings = settings.Value.EmailMappings;
        _defaultNamingStrategy = settings.Value.Storage?.FileSystem?.DefaultNamingStrategy ?? FileNamingStrategy.Original;
        _logger = logger;
    }

    public async Task<AttachmentStorageResult> SaveAttachmentAsync(
        AttachmentContext context, 
        byte[] data)
    {
        try
        {
            var folder = ResolveFolder(context.VendorEmail, context.SuggestedFolder);
            var fileName = GenerateFileName(context);
            var fullPath = Path.Combine(_baseDirectory, folder, fileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            
            await File.WriteAllBytesAsync(fullPath, data);
            
            _logger.LogInformation("Saved attachment: {FilePath} ({Size} bytes)", 
                fullPath, data.Length);
            
            return new AttachmentStorageResult
            {
                Success = true,
                StorageReference = fullPath,
                BytesWritten = data.Length,
                StorageType = StorageType.FileSystem
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment {FileName}", context.FileName);
            
            return new AttachmentStorageResult
            {
                Success = false,
                Error = ex.Message,
                StorageType = StorageType.FileSystem
            };
        }
    }

    public async Task<AttachmentStorageResult> SaveAttachmentStreamAsync(
        AttachmentContext context, 
        Stream dataStream)
    {
        try
        {
            var folder = ResolveFolder(context.VendorEmail, context.SuggestedFolder);
            var fileName = GenerateFileName(context);
            var fullPath = Path.Combine(_baseDirectory, folder, fileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await dataStream.CopyToAsync(fileStream);
            
            var bytesWritten = fileStream.Length;
            
            _logger.LogInformation("Saved attachment stream: {FilePath} ({Size} bytes)", 
                fullPath, bytesWritten);
            
            return new AttachmentStorageResult
            {
                Success = true,
                StorageReference = fullPath,
                BytesWritten = bytesWritten,
                StorageType = StorageType.FileSystem
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment stream {FileName}", context.FileName);
            
            return new AttachmentStorageResult
            {
                Success = false,
                Error = ex.Message,
                StorageType = StorageType.FileSystem
            };
        }
    }

    public async Task<byte[]> GetAttachmentAsync(string storageReference)
    {
        return await File.ReadAllBytesAsync(storageReference);
    }

    public Task<Stream> GetAttachmentStreamAsync(string storageReference)
    {
        return Task.FromResult<Stream>(new FileStream(storageReference, FileMode.Open, FileAccess.Read));
    }

    public Task<bool> ExistsAsync(string storageReference)
    {
        return Task.FromResult(File.Exists(storageReference));
    }

    public Task<bool> DeleteAttachmentAsync(string storageReference)
    {
        try
        {
            if (File.Exists(storageReference))
            {
                File.Delete(storageReference);
                _logger.LogInformation("Deleted attachment: {FilePath}", storageReference);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete attachment: {FilePath}", storageReference);
            return Task.FromResult(false);
        }
    }

    public Task<StorageStatistics> GetStatisticsAsync()
    {
        var stats = new StorageStatistics();
        
        foreach (var mapping in _emailMappings.SenderToFolderMap)
        {
            var folderPath = Path.Combine(_baseDirectory, mapping.Value);
            
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                stats.TotalFiles += files.Length;
                
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    stats.TotalSizeBytes += fileInfo.Length;
                    
                    if (!stats.FilesByVendor.ContainsKey(mapping.Key))
                        stats.FilesByVendor[mapping.Key] = 0;
                    
                    stats.FilesByVendor[mapping.Key]++;
                    
                    if (stats.OldestFile == null || fileInfo.CreationTime < stats.OldestFile)
                        stats.OldestFile = fileInfo.CreationTime;
                    
                    if (stats.NewestFile == null || fileInfo.CreationTime > stats.NewestFile)
                        stats.NewestFile = fileInfo.CreationTime;
                }
            }
        }
        
        return Task.FromResult(stats);
    }

    private string ResolveFolder(string vendorEmail, string? suggestedFolder)
    {
        if (!string.IsNullOrWhiteSpace(suggestedFolder))
            return suggestedFolder;
        
        if (_emailMappings.SenderToFolderMap.TryGetValue(vendorEmail, out var mappedFolder))
            return mappedFolder;
        
        var vendorName = vendorEmail.Split('@')[0];
        return $"invoices/{vendorName}";
    }

    private string GenerateFileName(AttachmentContext context)
    {
        var strategy = context.NamingStrategy ?? _defaultNamingStrategy;
        var baseName = Path.GetFileNameWithoutExtension(context.FileName);
        var extension = Path.GetExtension(context.FileName);
        
        return strategy switch
        {
            FileNamingStrategy.Original => context.FileName,
            
            FileNamingStrategy.WithTimestamp => 
                $"{baseName}_{context.EmailDate:yyyyMMdd_HHmmss}{extension}",
            
            FileNamingStrategy.WithSender =>
                $"{SanitizeFileName(context.VendorName)}_{baseName}{extension}",
            
            FileNamingStrategy.WithDateAndSender =>
                $"{context.EmailDate:yyyyMMdd}_{SanitizeFileName(context.VendorName)}_{baseName}{extension}",
            
            _ => context.FileName
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}
