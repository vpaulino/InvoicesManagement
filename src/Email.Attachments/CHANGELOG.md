# Changelog

All notable changes to the Email.Attachments package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2024-12-15

### Added
- Initial release of Email.Attachments
- High-level use-case oriented API via `IInvoiceManager`
  - `FetchLastWeekInvoicesAsync()` - Download last week's attachments
  - `FetchLastMonthInvoicesAsync()` - Download last month's attachments
  - `FetchLastQuarterInvoicesAsync()` - Download last quarter's attachments
  - `FetchLastYearInvoicesAsync()` - Download last year's attachments
  - `FetchInvoicesByVendorAsync()` - Download from specific vendor
  - `FetchInvoicesByPeriodAsync()` - Custom date range downloads
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
- Comprehensive documentation
  - API usage guide
  - Architecture overview
  - Date filtering guide
  - Quick reference cards

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
4. Create and push git tag: `git tag -a v1.0.0 -m "Release 1.0.0"`
5. GitHub Actions will automatically publish to NuGet.org

### Version Numbering

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR** - Incompatible API changes
- **MINOR** - New functionality, backward compatible
- **PATCH** - Bug fixes, backward compatible

---

[Unreleased]: https://github.com/vpaulino/InvoicesManagement/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/vpaulino/InvoicesManagement/releases/tag/v1.0.0
