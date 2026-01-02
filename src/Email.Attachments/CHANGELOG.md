# Changelog

All notable changes to the Email.Attachments package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - TBD

### ?? Breaking Changes
- **Service Registration:** Renamed `AddEmailFilesDownloader()` to `AddGmailFilesDownloader()` to explicitly indicate Gmail provider
- **Models:** Introduced provider-agnostic email models (`EmailMessage`, `EmailMessageDetails`, `AttachmentData`, `EmailMessagePart`)
- **IEmailService:** Updated to return provider-agnostic models instead of Gmail-specific types
- **EmailQuery:** Converted to abstract base class; use `GmailEmailQuery` for Gmail-specific queries
- **Service Names:** Renamed `GmailServiceWrapper` to `GmailService` for cleaner naming

### ? Added
- Multi-provider architecture with provider-agnostic core interfaces
- `EmailMessage` - Provider-agnostic message representation
- `EmailMessageDetails` - Detailed message with parsed headers
- `EmailMessagePart` - Message part/body representation
- `AttachmentData` - Provider-agnostic attachment data
- `EmailAddress` - Email address model
- `GmailEmailQuery` - Gmail-specific query implementation
- Automatic mapping between Gmail API models and provider-agnostic models

### ?? Changed
- `IEmailService` now returns provider-agnostic types
- `IAttachmentFilter` works with `EmailMessagePart` instead of Gmail `MessagePart`
- `GmailService` includes mapping logic from Gmail API to domain models
- Service registration explicitly requires Gmail configuration

### ?? Migration Guide (v1.x ? v2.0)

#### Service Registration
```csharp
// v1.x
builder.Services.AddEmailFilesDownloader(configuration);

// v2.0
builder.Services.AddGmailFilesDownloader(configuration);
```

#### High-Level API (No Changes Required!)
```csharp
// Still works exactly the same
var manager = services.GetRequiredService<IEmailFilesManager>();
var batch = await manager.FetchLastMonthEmailFilesAsync();
```

#### Direct IEmailService Usage (Breaking)
```csharp
// v1.x
var messages = await emailService.GetEmailsAsync(query);
// messages was IEnumerable<Google.Apis.Gmail.v1.Data.Message>

// v2.0
var gmailQuery = new GmailEmailQuery { SenderEmail = "..." };
var messages = await emailService.GetEmailsAsync(gmailQuery);
// messages is now IEnumerable<EmailMessage>
```

### ?? Benefits of v2.0
- ? Prepared for multiple email providers (Outlook, IMAP, etc.)
- ? Cleaner separation of concerns
- ? High-level APIs remain provider-agnostic
- ? Explicit provider selection at registration
- ? No Gmail types leaked into business logic

## [1.0.0] - 2024-12-15

### Added
- Initial release of Email.Attachments
- High-level use-case oriented API via `IEmailFilesManager`
  - `FetchLastWeekEmailFilesAsync()` - Download last week's attachments
  - `FetchLastMonthEmailFilesAsync()` - Download last month's attachments
  - `FetchLastQuarterEmailFilesAsync()` - Download last quarter's attachments
  - `FetchLastYearEmailFilesAsync()` - Download last year's attachments
  - `FetchEmailFilesByVendorAsync()` - Download from specific vendor
  - `FetchEmailFilesByPeriodAsync()` - Custom date range downloads
- Pluggable storage architecture
  - `IAttachmentPersistenceManager` interface
  - `FileSystemAttachmentPersistenceManager` implementation
  - Support for custom storage providers (Azure Blob, AWS S3, etc.)
- Comprehensive date filtering
  - Absolute dates (`After`, `Before`, `On`)
  - Relative periods (`NewerThan`, `OlderThan`)
  - Pre-built time period helpers (`TimePeriod` class)
- Flexible attachment handling strategies
  - `MetadataOnly` - Fast metadata retrieval without downloads
  - `LoadInMemory` - Load attachments into memory
  - `PersistAndReference` - Save to storage, return reference
  - `PersistAndLoad` - Save to storage and load in memory
- Gmail OAuth 2.0 integration
  - Automatic token refresh
  - Secure credential storage
- Rich metadata and statistics
  - Batch processing metrics
  - Vendor summaries
  - File size tracking
- File naming strategies
  - Original filename preservation
  - Timestamp-based naming
  - Sender-based naming
  - Date and sender combined naming
- Full dependency injection support
  - Microsoft.Extensions.Hosting integration
  - Service collection extensions
- Memory-efficient streaming for large attachments
- Base64url decoding for Gmail attachments
- Email body extraction (HTML and plain text)

### Dependencies
- Google.Apis.Gmail.v1 (>= 1.55.0.2510)
- Microsoft.Extensions.Hosting (>= 8.0.0)
- Microsoft.Extensions.Configuration.Binder (>= 8.0.0)
- Microsoft.Extensions.Configuration.EnvironmentVariables (>= 8.0.0)
- Microsoft.Extensions.Configuration.Json (>= 8.0.0)
- Microsoft.Extensions.Options (>= 8.0.0)

### Framework Support
- .NET 10.0

---

## Release Process

### Creating a Release

1. Update version in `Email.Attachments.csproj`
2. Update this CHANGELOG with release notes
3. Commit changes
4. Create and push git tag: `git tag -a v2.0.0 -m "Release 2.0.0"`
5. GitHub Actions will automatically publish to NuGet.org

### Version Numbering

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR** - Incompatible API changes
- **MINOR** - New functionality, backward compatible
- **PATCH** - Bug fixes, backward compatible

---

[Unreleased]: https://github.com/vpaulino/InvoicesManagement/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/vpaulino/InvoicesManagement/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/vpaulino/InvoicesManagement/releases/tag/v1.0.0
