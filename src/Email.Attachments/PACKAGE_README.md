# Email.Attachments (Gmail Provider)

[![NuGet](https://img.shields.io/nuget/v/Email.Attachments.svg)](https://www.nuget.org/packages/Email.Attachments/)
[![Downloads](https://img.shields.io/nuget/dt/Email.Attachments.svg)](https://www.nuget.org/packages/Email.Attachments/)
[![License](https://img.shields.io/github/license/vpaulino/InvoicesManagement)](LICENSE)

Comprehensive .NET library for automated email attachment extraction and management from Gmail accounts. Built with enterprise-grade architecture, featuring a high-level use-case oriented API, pluggable storage backends, and rich date filtering capabilities.

**Multi-Provider Architecture** - v2.0 introduces provider-agnostic core with Gmail as the first supported provider. Future providers (Outlook, etc.) can be easily added.

## ✨ Features

- 🎯 **Use-Case Oriented API** - Business-focused methods (`FetchLastMonthEmailFilesAsync`, `FetchLastQuarterEmailFilesAsync`, etc.)
- 💾 **Pluggable Storage** - FileSystem, Azure Blob, AWS S3, or custom implementations
- 📅 **Rich Date Filtering** - Absolute dates, relative periods, and time abstractions
- ⚡ **Memory Efficient** - Stream-based processing for large attachments
- 🔐 **OAuth 2.0** - Secure Gmail authentication with automatic token refresh
- 🏗️ **Full DI Support** - Built on Microsoft.Extensions.Hosting
- 📊 **Rich Metadata** - Batch statistics, vendor summaries, processing metrics

## 📦 Installation

```bash
dotnet add package Email.Attachments
```

**NuGet Package Manager:**
```powershell
Install-Package Email.Attachments
```

## 🚀 Quick Start

### 1. Setup Configuration

Create `appsettings.json`:

```json
{
  "googleCredentials": {
    "Values": {
      "credentialsLocation": "credentials.json",
      "tokenDestination": "token.json"
    }
  },
  "emailsAttachmentsDestination": {
    "Values": {
      "billing@vendor.com": "invoices"
    }
  },
  "storage": {
    "defaultStorageType": "FileSystem",
    "fileSystem": {
      "baseDirectory": "./attachments",
      "defaultNamingStrategy": "WithDateAndSender"
    }
  }
}
```

### 2. Register Gmail Services

```csharp
using ExtractLoadInvoices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register Gmail provider services
builder.Services.AddGmailFilesDownloader(builder.Configuration);

var host = builder.Build();
```

### 3. Use High-Level API

```csharp
using Email.Attachments.UseCases;

var manager = host.Services.GetRequiredService<IEmailFilesManager>();

// Download last month's attachments
var batch = await manager.FetchLastMonthEmailFilesAsync();

Console.WriteLine($"Downloaded {batch.Metadata.TotalAttachments} attachments");
Console.WriteLine($"Total size: {batch.Metadata.TotalSizeBytes / 1024.0 / 1024.0:F2} MB");
```

## 📚 Usage Examples

### Download Last Week's Attachments

```csharp
var batch = await manager.FetchLastWeekEmailFilesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
    NamingStrategy = FileNamingStrategy.WithDateAndSender
});

foreach (var emailFile in batch.EmailAttachments)
{
    Console.WriteLine($"{emailFile.SentDate:yyyy-MM-dd} | {emailFile.Sender}");
    foreach (var attachment in emailFile.Attachments)
    {
        Console.WriteLine($"  ✓ {attachment.StorageReference}");
    }
}
```

### Generate Metadata Report (No Downloads)

```csharp
var batch = await manager.FetchLastMonthEmailFilesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly
});

// Fast! Only fetches metadata, no file downloads
foreach (var vendor in batch.Metadata.InvoicesByVendor)
{
    Console.WriteLine($"{vendor.Key}: {vendor.Value} emails");
}
```

### Custom Date Range

```csharp
var period = TimePeriod.Custom(
    new DateTime(2024, 1, 1),
    new DateTime(2024, 3, 31),
    "Q1 2024");

var batch = await manager.FetchEmailFilesByPeriodAsync(period);
```

### Vendor-Specific Download

```csharp
var batch = await manager.FetchEmailFilesByVendorAsync(
    "billing@company.com",
    TimePeriod.LastQuarter());
```

## 🎯 Attachment Handling Strategies

| Strategy | Downloads? | In Memory? | Use Case |
|----------|------------|------------|----------|
| `MetadataOnly` | ❌ | ❌ | Fast reports |
| `LoadInMemory` | ✅ | ✅ | Immediate processing |
| `PersistAndReference` | ✅ | ❌ | Long-term storage (recommended) |
| `PersistAndLoad` | ✅ | ✅ | Process + archive |

## 🏗️ Architecture

```
IEmailFilesManager (High-Level API - Provider Agnostic)
    ├─► Time period methods (FetchLastWeekEmailFilesAsync, FetchLastMonthEmailFilesAsync, etc.)
    ├─► Vendor operations (FetchEmailFilesByVendorAsync, FetchEmailFilesByVendorsAsync)
    └─► Custom queries (FetchEmailFilesByPeriodAsync)
         ↓
Uses provider-agnostic services:
    ├─► IEmailService (Gmail implementation: GmailService)
    ├─► IAttachmentPersistenceManager (Storage)
    └─► IAttachmentDownloader (Download & decode)
```

## 📧 Email Provider Support

### Gmail (Current)
- ✅ Fully supported
- ✅ OAuth 2.0 authentication
- ✅ Advanced query syntax
- ✅ Label/folder support

### Future Providers
- 🔜 Microsoft Outlook/Exchange (planned)
- 🔜 Custom IMAP providers (planned)

## 📅 Time Period Helpers

```csharp
TimePeriod.LastWeek()      // Previous Monday-Sunday
TimePeriod.LastMonth()     // Previous calendar month
TimePeriod.LastQuarter()   // Previous Q1, Q2, Q3, or Q4
TimePeriod.LastYear()      // Previous calendar year
TimePeriod.LastNDays(30)   // Last 30 days
TimePeriod.Custom(start, end, description)
```

## 🔐 Google OAuth Setup

1. Create a project in [Google Cloud Console](https://console.cloud.google.com/)
2. Enable Gmail API
3. Create OAuth 2.0 credentials (Desktop app)
4. Download `credentials.json`
5. Place in your application directory

## 💾 Storage Providers

### FileSystem (Built-in)
```json
"storage": {
  "defaultStorageType": "FileSystem",
  "fileSystem": {
    "baseDirectory": "./attachments",
    "defaultNamingStrategy": "WithDateAndSender"
  }
}
```

### Custom Provider
Implement `IAttachmentPersistenceManager`:

```csharp
public class AzureBlobStorage : IAttachmentPersistenceManager
{
    // Your implementation
}
```

## 🎨 File Naming Strategies

- `Original` - Keep original filename
- `WithTimestamp` - `invoice_20241215_143022.pdf`
- `WithSender` - `vendorname_invoice.pdf`
- `WithDateAndSender` - `20241215_vendorname_invoice.pdf` (recommended)

## 📊 Rich Metadata

```csharp
batch.Metadata.TotalEmails
batch.Metadata.TotalAttachments
batch.Metadata.TotalSizeBytes
batch.Metadata.ProcessingTime
batch.Metadata.InvoicesByVendor  // Dictionary<vendor, count>
```

## 🔧 Requirements

- .NET 10.0 or later
- Google OAuth 2.0 credentials
- Gmail account with API access enabled

## 📖 Documentation

- [Complete API Guide](https://github.com/vpaulino/InvoicesManagement/blob/main/EmailFilesDownloader/Email.Attachments/USE_CASE_API_GUIDE.md)
- [Date Filtering Guide](https://github.com/vpaulino/InvoicesManagement/blob/main/EmailFilesDownloader/Email.Attachments/DATE_FILTERING_GUIDE.md)
- [Architecture Overview](https://github.com/vpaulino/InvoicesManagement/blob/main/EmailFilesDownloader/Email.Attachments/ARCHITECTURE.md)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with:
- [Google.Apis.Gmail](https://www.nuget.org/packages/Google.Apis.Gmail.v1/)
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/)

## 📞 Support

- 🐛 [Report Issues](https://github.com/vpaulino/InvoicesManagement/issues)
- 💬 [Discussions](https://github.com/vpaulino/InvoicesManagement/discussions)
- 📧 Contact: [Your Email]

---

**Made with ❤️ by vpaulino**
