using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.Authentication;
using ExtractLoadInvoices.Configuration;
using ExtractLoadInvoices.FileSystem;
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Storage;
using ExtractLoadInvoices.UseCases;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExtractLoadInvoices;

/// <summary>
/// Extension methods for registering invoice downloader services
/// </summary>
public static class InvoiceDownloaderServiceCollectionExtensions
{
    /// <summary>
    /// Registers all invoice downloader services into the dependency injection container
    /// </summary>
    public static IServiceCollection AddInvoiceDownloader(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<AppSettings>(options =>
        {
            // Bind root configuration
            options.ApplicationName = configuration.GetValue<string>("applicationName") ?? "InvoiceDownloader";
            
            // Bind Google Credentials
            var googleCreds = new Dictionary<string, string>();
            configuration.GetSection("googleCredentials:Values").Bind(googleCreds);
            options.GoogleCredentials = new GoogleCredentialsSettings
            {
                CredentialsLocation = googleCreds.GetValueOrDefault("credentialsLocation") ?? "credentials.json",
                TokenDestination = googleCreds.GetValueOrDefault("tokenDestination") ?? "token.json"
            };
            
            // Bind Email Mappings
            var emailMappings = new Dictionary<string, string>();
            configuration.GetSection("emailsAttachmentsDestination:Values").Bind(emailMappings);
            options.EmailMappings = new EmailMappingSettings
            {
                SenderToFolderMap = emailMappings
            };
            
            // Bind Storage Settings
            var storageType = configuration.GetValue<string>("storage:defaultStorageType");
            if (Enum.TryParse<StorageType>(storageType, out var parsedType))
            {
                options.Storage = new StorageSettings
                {
                    DefaultStorageType = parsedType,
                    FileSystem = new FileSystemStorageSettings
                    {
                        BaseDirectory = configuration.GetValue<string>("storage:fileSystem:baseDirectory") ?? "./invoices",
                        DefaultNamingStrategy = Enum.TryParse<FileNamingStrategy>(
                            configuration.GetValue<string>("storage:fileSystem:defaultNamingStrategy"),
                            out var namingStrategy) ? namingStrategy : FileNamingStrategy.WithDateAndSender,
                        CreateVendorSubfolders = configuration.GetValue<bool>("storage:fileSystem:createVendorSubfolders")
                    }
                };
            }
        });
        
        // Register core services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IGoogleAuthenticator, GoogleAuthenticator>();
        services.AddSingleton<IAttachmentFilter, AttachmentFilter>();
        services.AddSingleton<IAttachmentDownloader, AttachmentDownloader>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IEmailService, GmailServiceWrapper>();
        services.AddSingleton<IEmailProcessor, EmailProcessor>();
        
        // Register storage manager based on configuration
        services.AddSingleton<IAttachmentPersistenceManager>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            var logger = sp.GetRequiredService<ILogger<FileSystemAttachmentPersistenceManager>>();
            
            return settings.Storage.DefaultStorageType switch
            {
                StorageType.FileSystem => 
                    new FileSystemAttachmentPersistenceManager(
                        sp.GetRequiredService<IOptions<AppSettings>>(), 
                        logger),
                
                _ => throw new NotSupportedException(
                    $"Storage type {settings.Storage.DefaultStorageType} not yet implemented")
            };
        });
        
        // Register high-level Invoice Manager
        services.AddSingleton<IEmailFilesManager, EmailFilesManager>();

        // Register Gmail service
        services.AddSingleton(serviceProvider =>
        {
            var configService = serviceProvider.GetRequiredService<IConfigurationService>();
            var settings = configService.LoadConfiguration();
            var authenticator = serviceProvider.GetRequiredService<IGoogleAuthenticator>();

            string[] scopes = { 
                GmailService.Scope.GmailReadonly, 
                GmailService.Scope.GmailLabels, 
                GmailService.Scope.GmailModify 
            };

            var credential = authenticator.AuthenticateAsync(
                settings.GoogleCredentials.CredentialsLocation,
                settings.GoogleCredentials.TokenDestination,
                scopes).GetAwaiter().GetResult();

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = settings.ApplicationName
            });
        });

        return services;
    }
}
