# ?? Use-Case Oriented API - Implementation Complete!

## ? What Was Implemented

A complete **high-level, use-case oriented API** for invoice management with **pluggable storage backends**.

---

## ?? **New Files Created**

### **Storage Layer** (`Storage/`)
- `IAttachmentPersistenceManager.cs` - Storage abstraction interface
- `AttachmentContext.cs` - Attachment metadata for storage
- `StorageModels.cs` - Storage result models and enums
- `FileSystemAttachmentPersistenceManager.cs` - FileSystem implementation

### **Use Case Layer** (`UseCases/`)
- `IInvoiceManager.cs` - High-level API interface
- `InvoiceManager.cs` - Implementation with business logic
- `TimePeriod.cs` - Time period abstractions (Last Week, Last Month, etc.)
- `FetchOptions.cs` - Flexible fetch configuration
- `InvoiceModels.cs` - Invoice, InvoiceBatch, VendorInfo models

### **Configuration** (`Configuration/`)
- `StorageSettings.cs` - Storage configuration models
- Updated `AppSettings.cs` - Added Storage property

### **Updates**
- `app.json` - Added storage configuration
- `InvoiceDownloaderServiceCollectionExtensions.cs` - Registered new services

---

## ?? **High-Level API Overview**

### **Time Period Operations**

```csharp
var manager = serviceProvider.GetRequiredService<IInvoiceManager>();

// Last week's invoices
await manager.FetchLastWeekInvoicesAsync();

// This month's invoices
await manager.FetchThisMonthInvoicesAsync();

// Last quarter's invoices
await manager.FetchLastQuarterInvoicesAsync();

// Last year's invoices (tax year)
await manager.FetchLastYearInvoicesAsync();

// Last N days
await manager.FetchLastNDaysInvoicesAsync(30);

// Custom period
await manager.FetchInvoicesByPeriodAsync(
    TimePeriod.Custom(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31)));
```

---

## ?? **Usage Examples**

### **Example 1: Metadata Only (No Downloads) - Fast Reports**

```csharp
var manager = serviceProvider.GetRequiredService<IInvoiceManager>();

var batch = await manager.FetchLastMonthInvoicesAsync(new FetchOptions
{
    IncludeMetadata = true,
    IncludeEmailBody = false,
    IncludeAttachments = true,   // Get attachment info
    AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly  // Don't download
});

// Generate report
Console.WriteLine($"Period: {batch.Metadata.Period?.Description}");
Console.WriteLine($"Total invoices: {batch.Metadata.TotalInvoices}");
Console.WriteLine($"Total size: {batch.Metadata.TotalSizeBytes / 1024 / 1024} MB");

foreach (var invoice in batch.Invoices)
{
    Console.WriteLine($"{invoice.SentDate:yyyy-MM-dd} | {invoice.Sender} | {invoice.Subject}");
    foreach (var att in invoice.Attachments)
    {
        Console.WriteLine($"  - {att.FileName} ({att.FileSize} bytes) [NOT DOWNLOADED]");
    }
}

// By vendor report
foreach (var vendor in batch.Metadata.InvoicesByVendor)
{
    Console.WriteLine($"{vendor.Key}: {vendor.Value} invoices");
}
```

---

### **Example 2: Download and Persist to FileSystem (Default)**

```csharp
var batch = await manager.FetchLastWeekInvoicesAsync(new FetchOptions
{
    IncludeMetadata = true,
    IncludeAttachments = true,
    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference  // DEFAULT
});

foreach (var invoice in batch.Invoices)
{
    Console.WriteLine($"{invoice.SentDate:yyyy-MM-dd} | {invoice.Sender}");
    
    foreach (var attachment in invoice.Attachments)
    {
        if (attachment.IsPersisted)
        {
            Console.WriteLine($"  Saved: {attachment.StorageReference}");
            // Example: D:/invoices/aguas/20241215_aquamatrix_invoice.pdf
        }
    }
}
```

---

### **Example 3: Load in Memory for Processing**

```csharp
var batch = await manager.FetchLastWeekInvoicesAsync(new FetchOptions
{
    IncludeMetadata = true,
    IncludeAttachments = true,
    AttachmentStrategy = AttachmentHandlingStrategy.LoadInMemory  // Keep in RAM
});

foreach (var invoice in batch.Invoices)
{
    foreach (var attachment in invoice.Attachments)
    {
        if (attachment.IsInMemory && attachment.Data != null)
        {
            // Process PDF in memory (extract text, parse amounts, etc.)
            var text = ExtractPdfText(attachment.Data);
            Console.WriteLine($"Extracted: {text}");
        }
    }
}
```

---

### **Example 4: Custom Storage Manager**

```csharp
// Create custom storage manager
var customStorage = new AzureBlobAttachmentPersistenceManager(...);

var batch = await manager.FetchLastQuarterInvoicesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
    PersistenceManager = customStorage,  // Override DI default
    DestinationFolder = "q4-2024"
});
```

---

### **Example 5: Vendor-Specific Downloads**

```csharp
// Download from specific vendor
var batch = await manager.FetchInvoicesByVendorAsync(
    "noreply.simar@aquamatrix.pt",
    TimePeriod.LastMonth());

Console.WriteLine($"Downloaded {batch.Metadata.TotalInvoices} invoices from Aquamatrix");
```

---

### **Example 6: Multiple Vendors, Custom Period**

```csharp
var vendors = new[] { "vendor1@company.com", "vendor2@company.com" };

var batch = await manager.FetchInvoicesByVendorsAsync(
    vendors,
    TimePeriod.Custom(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31), "Q1 2024"),
    new FetchOptions
    {
        IncludeEmailBody = true,  // Include full email content
        NamingStrategy = FileNamingStrategy.WithDateAndSender
    });
```

---

### **Example 7: Get Configured Vendors**

```csharp
var vendors = await manager.GetConfiguredVendorsAsync();

foreach (var vendor in vendors)
{
    Console.WriteLine($"Vendor: {vendor.Name} ({vendor.Email})");
}
```

---

### **Example 8: Retrieve Saved Attachments Later**

```csharp
var persistenceManager = serviceProvider.GetRequiredService<IAttachmentPersistenceManager>();

// Get attachment data
var data = await persistenceManager.GetAttachmentAsync(
    "D:/invoices/aguas/20241215_invoice.pdf");

// Or as stream (memory efficient)
using var stream = await persistenceManager.GetAttachmentStreamAsync(
    "D:/invoices/aguas/20241215_invoice.pdf");

// Storage statistics
var stats = await persistenceManager.GetStatisticsAsync();
Console.WriteLine($"Total files: {stats.TotalFiles}");
Console.WriteLine($"Total size: {stats.TotalSizeBytes / 1024 / 1024} MB");
```

---

## ?? **AttachmentHandlingStrategy Comparison**

| Strategy | Downloads? | In Memory? | Persisted? | Use Case |
|----------|------------|------------|------------|----------|
| `MetadataOnly` | ? No | ? No | ? No | **Fast reports, listings** |
| `LoadInMemory` | ? Yes | ? Yes | ? No | **Immediate processing** |
| `PersistAndReference` | ? Yes | ? No | ? Yes | **Long-term storage (RECOMMENDED)** |
| `PersistAndLoad` | ? Yes | ? Yes | ? Yes | **Storage + immediate processing** |

---

## ??? **Architecture Layers**

```
???????????????????????????????????????????????????
?      Your Application                           ?
?  (Uses IInvoiceManager - business-focused)      ?
???????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????
?       IInvoiceManager                           ?
?  • FetchLastWeekInvoicesAsync()                 ?
?  • FetchByVendorAsync()                         ?
?  • Period abstractions                          ?
???????????????????????????????????????????????????
                   ? Uses
         ?????????????????????
         ?         ?         ?
    ?????????? ?????????? ????????????????
    ?IGmail  ? ?IAttach ? ?IPersistence  ?
    ?Service ? ?Download? ?Manager       ?
    ?????????? ?????????? ????????????????
                                  ?
                     ???????????????????????????
                     ?            ?            ?
               ???????????  ???????????  ???????????
               ?FileSystem?  ?Azure Blob?  ?AWS S3  ?
               ???????????  ???????????  ???????????
```

---

## ?? **Configuration (app.json)**

```json
{
  "applicationName": "InvoiceDownloader",
  
  "googleCredentials": {
    "Values": {
      "credentialsLocation": "credentials.json",
      "tokenDestination": "token.json"
    }
  },
  
  "emailsAttachmentsDestination": {
    "Values": {
      "noreply.simar@aquamatrix.pt": "aguas",
      "billing@vendor.com": "vendor-invoices"
    }
  },
  
  "storage": {
    "defaultStorageType": "FileSystem",
    "fileSystem": {
      "baseDirectory": "./invoices",
      "defaultNamingStrategy": "WithDateAndSender",
      "createVendorSubfolders": true
    }
  }
}
```

---

## ?? **Key Benefits**

### ? **Business-Focused API**
- Speak in domain terms: "last week", "this quarter", "vendor"
- No need to calculate dates manually
- No need to understand Gmail API details

### ? **Flexible Data Retrieval**
- Choose what you need: metadata, body, attachments
- Control memory usage with AttachmentHandlingStrategy
- Pay for what you use (performance)

### ? **Pluggable Storage**
- FileSystem (implemented)
- Azure Blob Storage (future)
- AWS S3 (future)
- Your custom implementation

### ? **Memory Efficient**
- Stream-based APIs
- No large byte arrays unless requested
- Configurable attachment handling

### ? **Rich Metadata**
- Batch statistics
- Vendor summaries
- Processing time metrics

---

## ?? **Comparison: Old vs New API**

| Task | Old API | New API |
|------|---------|---------|
| **Last week invoices** | Manual date calc + EmailQuery | `FetchLastWeekInvoicesAsync()` |
| **Metadata only** | Not supported | `AttachmentStrategy.MetadataOnly` |
| **Email body** | Not accessible | `IncludeEmailBody = true` |
| **Custom storage** | Hard-coded FileSystem | `IAttachmentPersistenceManager` |
| **Vendor report** | Manual grouping | `BatchMetadata.InvoicesByVendor` |
| **Memory control** | Always loads data | `AttachmentHandlingStrategy` enum |

---

## ?? **Next Steps**

1. **Try the examples** above in your application
2. **Customize `FetchOptions`** for your use case
3. **Implement custom storage** (Azure Blob, AWS S3) if needed
4. **Add business logic** (parse invoice numbers, extract amounts)
5. **Build automation** (scheduled jobs, background services)

---

## ?? **Related Documentation**

- `DATE_FILTERING_GUIDE.md` - Date filtering capabilities
- `ARCHITECTURE.md` - Overall architecture
- `README.md` - Project overview

---

**Your invoice management is now enterprise-ready with a clean, use-case oriented API!** ??
