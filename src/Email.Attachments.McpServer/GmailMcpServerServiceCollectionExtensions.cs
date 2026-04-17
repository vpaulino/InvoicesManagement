using EmailAttachments.McpServer.Authentication;
using EmailAttachments.McpServer.Handlers;
using EmailAttachments.McpServer.Options;
using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.Authentication;
using ExtractLoadInvoices.Configuration;
using ExtractLoadInvoices.FileSystem;
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Storage;
using ExtractLoadInvoices.UseCases;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using GmailApi = Google.Apis.Gmail.v1;

namespace EmailAttachments.McpServer;

/// <summary>
/// Extension methods for registering the Gmail MCP server into an ASP.NET
/// dependency-injection container.
/// </summary>
public static class GmailMcpServerServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services required to run a Gmail MCP server and mounts the
    /// MCP Streamable-HTTP transport. Call <see cref="GmailMcpServerEndpointRouteBuilderExtensions.MapGmailMcpServer"/>
    /// on the built application to map the endpoint.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="configureOptions">
    /// A delegate used to configure <see cref="GmailMcpServerOptions"/>. At a
    /// minimum, set <see cref="GmailMcpServerOptions.AuthMode"/>.
    /// </param>
    /// <param name="configuration">
    /// Optional root <see cref="IConfiguration"/> used to bind storage and
    /// vendor-mapping settings. When omitted the package uses sensible defaults.
    /// </param>
    /// <returns>The service collection for further chaining.</returns>
    public static IServiceCollection AddGmailMcpServer(
        this IServiceCollection services,
        Action<GmailMcpServerOptions> configureOptions,
        IConfiguration? configuration = null)
    {
        // ── 1. Capture options early so we can branch on AuthMode ─────────────
        var opts = new GmailMcpServerOptions();
        configureOptions(opts);

        services.Configure<GmailMcpServerOptions>(configureOptions);

        // ── 2. Bind AppSettings (vendor mappings, storage, etc.) ──────────────
        if (configuration is not null)
        {
            services.Configure<AppSettings>(appSettings =>
            {
                appSettings.ApplicationName =
                    configuration.GetValue<string>("applicationName")
                    ?? opts.ApplicationName;

                var googleCreds = new Dictionary<string, string>();
                configuration.GetSection("googleCredentials:Values").Bind(googleCreds);
                appSettings.GoogleCredentials = new GoogleCredentialsSettings
                {
                    CredentialsLocation =
                        googleCreds.GetValueOrDefault("credentialsLocation")
                        ?? opts.CredentialsPath,
                    TokenDestination =
                        googleCreds.GetValueOrDefault("tokenDestination")
                        ?? opts.TokenPath
                };

                var emailMappings = new Dictionary<string, string>();
                configuration.GetSection("emailsAttachmentsDestination:Values")
                             .Bind(emailMappings);
                appSettings.EmailMappings = new EmailMappingSettings
                {
                    SenderToFolderMap = emailMappings
                };

                var storageType =
                    configuration.GetValue<string>("storage:defaultStorageType");
                appSettings.Storage = new StorageSettings
                {
                    DefaultStorageType =
                        Enum.TryParse<StorageType>(storageType, out var parsed)
                            ? parsed
                            : StorageType.FileSystem,
                    FileSystem = new FileSystemStorageSettings
                    {
                        BaseDirectory =
                            configuration.GetValue<string>(
                                "storage:fileSystem:baseDirectory")
                            ?? opts.BaseStorageDirectory,
                        DefaultNamingStrategy =
                            Enum.TryParse<FileNamingStrategy>(
                                configuration.GetValue<string>(
                                    "storage:fileSystem:defaultNamingStrategy"),
                                out var ns)
                                ? ns
                                : FileNamingStrategy.WithDateAndSender
                    }
                };
            });
        }
        else
        {
            services.Configure<AppSettings>(appSettings =>
            {
                appSettings.ApplicationName = opts.ApplicationName;
                appSettings.GoogleCredentials = new GoogleCredentialsSettings
                {
                    CredentialsLocation = opts.CredentialsPath,
                    TokenDestination = opts.TokenPath
                };
                appSettings.Storage = new StorageSettings
                {
                    DefaultStorageType = StorageType.FileSystem,
                    FileSystem = new FileSystemStorageSettings
                    {
                        BaseDirectory = opts.BaseStorageDirectory,
                        DefaultNamingStrategy = FileNamingStrategy.WithDateAndSender
                    }
                };
            });
        }

        // ── 3. Core Email.Attachments services ────────────────────────────────
        //      Register with the appropriate lifetime depending on auth mode.
        //      ServiceAccount → singletons (one Gmail identity for all requests)
        //      UserDelegated  → scoped    (one Gmail identity per HTTP request)
        if (opts.AuthMode == GmailAuthMode.ServiceAccount)
        {
            RegisterServiceAccountMode(services);
        }
        else
        {
            RegisterUserDelegatedMode(services);
        }

        // ── 4. MCP server ─────────────────────────────────────────────────────
        services
            .AddMcpServer(mcpOptions =>
            {
                mcpOptions.ServerInfo = new ModelContextProtocol.Protocol.Implementation
                {
                    Name = opts.ServerName,
                    Version = opts.ServerVersion
                };
            })
            .WithHttpTransport()
            .WithTools<GmailMcpTools>();

        return services;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static void RegisterServiceAccountMode(IServiceCollection services)
    {
        // Credential provider — singleton, credentials loaded once at startup
        services.AddSingleton<IGoogleAuthenticator, GoogleAuthenticator>();
        services.AddSingleton<IGmailCredentialProvider, ServiceAccountGmailCredentialProvider>();

        // Gmail API service — singleton, same credential for all requests
        services.AddSingleton(sp =>
        {
            var credProvider = sp.GetRequiredService<IGmailCredentialProvider>();
            var mcpOptions = sp.GetRequiredService<IOptions<GmailMcpServerOptions>>().Value;

            var credential = credProvider.GetCredentialAsync().GetAwaiter().GetResult();

            return new GmailApi.GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = mcpOptions.ApplicationName
            });
        });

        RegisterEmailServices(services, singleton: true);
    }

    private static void RegisterUserDelegatedMode(IServiceCollection services)
    {
        // HttpContextAccessor is needed to read the bearer token per-request
        services.AddHttpContextAccessor();

        // Credential provider — scoped, reads token from the current HTTP context
        services.AddScoped<IGmailCredentialProvider, UserDelegatedGmailCredentialProvider>();

        // Gmail API service — scoped, fresh credential per request
        services.AddScoped(sp =>
        {
            var credProvider = sp.GetRequiredService<IGmailCredentialProvider>();
            var mcpOptions = sp.GetRequiredService<IOptions<GmailMcpServerOptions>>().Value;

            var credential = credProvider.GetCredentialAsync().GetAwaiter().GetResult();

            return new GmailApi.GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = mcpOptions.ApplicationName
            });
        });

        RegisterEmailServices(services, singleton: false);
    }

    /// <summary>
    /// Registers the Email.Attachments service stack with either singleton or
    /// scoped lifetimes, depending on <paramref name="singleton"/>.
    /// </summary>
    private static void RegisterEmailServices(IServiceCollection services, bool singleton)
    {
        if (singleton)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IAttachmentFilter, AttachmentFilter>();
            services.AddSingleton<IAttachmentDownloader, AttachmentDownloader>();
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
            services.AddSingleton<IEmailService, GmailService>();
            services.AddSingleton<IEmailProcessor, EmailProcessor>();
            services.AddSingleton<IAttachmentPersistenceManager>(sp =>
                BuildPersistenceManager(sp, singleton: true));
            services.AddSingleton<IEmailFilesManager, EmailFilesManager>();
        }
        else
        {
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IAttachmentFilter, AttachmentFilter>();
            services.AddScoped<IAttachmentDownloader, AttachmentDownloader>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IEmailService, GmailService>();
            services.AddScoped<IEmailProcessor, EmailProcessor>();
            services.AddScoped<IAttachmentPersistenceManager>(sp =>
                BuildPersistenceManager(sp, singleton: false));
            services.AddScoped<IEmailFilesManager, EmailFilesManager>();
        }
    }

    private static IAttachmentPersistenceManager BuildPersistenceManager(
        IServiceProvider sp, bool singleton)
    {
        var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
        var logger = sp.GetRequiredService<
            ILogger<FileSystemAttachmentPersistenceManager>>();

        return settings.Storage.DefaultStorageType switch
        {
            StorageType.FileSystem =>
                new FileSystemAttachmentPersistenceManager(
                    sp.GetRequiredService<IOptions<AppSettings>>(),
                    logger),

            _ => throw new NotSupportedException(
                $"Storage type '{settings.Storage.DefaultStorageType}' is not " +
                "supported by Email.Attachments.McpServer. " +
                "Implement IAttachmentPersistenceManager and register it manually.")
        };
    }
}
