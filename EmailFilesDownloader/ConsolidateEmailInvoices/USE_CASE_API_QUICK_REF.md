# ?? Quick Reference - Use-Case Oriented API

## ? Quick Start

```csharp
var manager = serviceProvider.GetRequiredService<IInvoiceManager>();
var batch = await manager.FetchLastWeekInvoicesAsync();
Console.WriteLine($"Downloaded {batch.Metadata.TotalInvoices} invoices");
```

---

## ?? Time Period Methods

| Method | Period | Example |
|--------|--------|---------|
| `FetchLastWeekInvoicesAsync()` | Last Mon-Sun | Previous week |
| `FetchThisWeekInvoicesAsync()` | Current Mon-Sun | This week |
| `FetchLastMonthInvoicesAsync()` | Previous month | November if now is December |
| `FetchThisMonthInvoicesAsync()` | Current month | December if now is December |
| `FetchLastQuarterInvoicesAsync()` | Previous quarter | Q3 if now is Q4 |
| `FetchThisQuarterInvoicesAsync()` | Current quarter | Q4 if now is Q4 |
| `FetchLastYearInvoicesAsync()` | Previous year | 2023 if now is 2024 |
| `FetchThisYearInvoicesAsync()` | Current year | 2024 |
| `FetchLastNDaysInvoicesAsync(30)` | Last N days | Last 30 days |

---

## ?? AttachmentHandlingStrategy

| Strategy | Memory | Disk | Use When |
|----------|--------|------|----------|
| `MetadataOnly` | ? Low | ? No | Generating reports |
| `LoadInMemory` | ?? High | ? No | Processing PDFs immediately |
| `PersistAndReference` ? | ? Low | ? Yes | **Recommended default** |
| `PersistAndLoad` | ?? High | ? Yes | Process + archive |

---

## ?? Common Scenarios

### **Scenario 1: Monthly Report (No Downloads)**
```csharp
var batch = await manager.FetchLastMonthInvoicesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly
});
// Fast! Only metadata, no file downloads
```

### **Scenario 2: Download Last Week's Invoices**
```csharp
var batch = await manager.FetchLastWeekInvoicesAsync();
// Uses default: PersistAndReference
// Files saved to disk, paths in attachment.StorageReference
```

### **Scenario 3: Process PDFs Immediately**
```csharp
var batch = await manager.FetchLastWeekInvoicesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.LoadInMemory
});

foreach (var invoice in batch.Invoices)
{
    foreach (var att in invoice.Attachments)
    {
        ProcessPdf(att.Data);  // Data is in memory
    }
}
```

### **Scenario 4: Vendor-Specific Download**
```csharp
var batch = await manager.FetchInvoicesByVendorAsync(
    "billing@company.com",
    TimePeriod.LastMonth());
```

### **Scenario 5: Year-End Tax Collection**
```csharp
var batch = await manager.FetchLastYearInvoicesAsync(new FetchOptions
{
    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
    DestinationFolder = "tax-2023",
    NamingStrategy = FileNamingStrategy.WithDateAndSender,
    UnreadOnly = false  // Include all emails
});
```

---

## ?? FetchOptions Properties

```csharp
new FetchOptions
{
    // What to include
    IncludeMetadata = true,      // Date, sender, subject
    IncludeEmailBody = false,    // Full email HTML/text
    IncludeAttachments = true,   // Attachment info
    
    // How to handle attachments
    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
    
    // Storage options
    PersistenceManager = customStorage,  // Override DI default
    DestinationFolder = "my-folder",     // Override email mapping
    NamingStrategy = FileNamingStrategy.WithDateAndSender,
    
    // Filters
    UnreadOnly = true,           // Only unread emails
    MarkAsRead = true,           // Mark as read after processing
    
    // Performance
    MaxResults = 100,            // Limit results
    ParallelDownload = false     // Download in parallel (future)
}
```

---

## ??? FileNamingStrategy

| Strategy | Example Output |
|----------|----------------|
| `Original` | `invoice.pdf` |
| `WithTimestamp` | `invoice_20241215_143022.pdf` |
| `WithSender` | `aquamatrix_invoice.pdf` |
| `WithDateAndSender` | `20241215_aquamatrix_invoice.pdf` ? |

---

## ?? Batch Metadata

```csharp
var batch = await manager.FetchLastMonthInvoicesAsync();

Console.WriteLine($"Period: {batch.Metadata.Period.Description}");
Console.WriteLine($"Total invoices: {batch.Metadata.TotalInvoices}");
Console.WriteLine($"Total attachments: {batch.Metadata.TotalAttachments}");
Console.WriteLine($"Total size: {batch.Metadata.TotalSizeBytes / 1024 / 1024} MB");
Console.WriteLine($"Processing time: {batch.Metadata.ProcessingTime.TotalSeconds}s");

// By vendor
foreach (var vendor in batch.Metadata.InvoicesByVendor)
{
    Console.WriteLine($"{vendor.Key}: {vendor.Value} invoices");
}
```

---

## ?? Invoice Properties

```csharp
foreach (var invoice in batch.Invoices)
{
    // Metadata
    invoice.MessageId
    invoice.SentDate
    invoice.Sender
    invoice.SenderName
    invoice.Subject
    
    // Content (if IncludeEmailBody = true)
    invoice.EmailBody
    invoice.EmailBodyPlainText
    
    // Attachments
    foreach (var att in invoice.Attachments)
    {
        att.FileName
        att.FileSize
        att.MimeType
        att.IsPersisted        // true if saved to disk
        att.StorageReference   // File path or URL
        att.StorageType        // FileSystem, AzureBlob, etc.
        att.Data               // byte[] if in memory
    }
}
```

---

## ?? Storage Operations

```csharp
var storage = serviceProvider.GetRequiredService<IAttachmentPersistenceManager>();

// Retrieve attachment
var data = await storage.GetAttachmentAsync("path/to/file.pdf");

// Stream (memory efficient)
using var stream = await storage.GetAttachmentStreamAsync("path/to/file.pdf");

// Check if exists
bool exists = await storage.ExistsAsync("path/to/file.pdf");

// Delete
bool deleted = await storage.DeleteAttachmentAsync("path/to/file.pdf");

// Statistics
var stats = await storage.GetStatisticsAsync();
```

---

## ?? TimePeriod Helpers

```csharp
// Pre-defined periods
TimePeriod.LastWeek()
TimePeriod.ThisWeek()
TimePeriod.LastMonth()
TimePeriod.ThisMonth()
TimePeriod.LastQuarter()
TimePeriod.ThisQuarter()
TimePeriod.LastYear()
TimePeriod.ThisYear()
TimePeriod.LastNDays(30)

// Custom period
TimePeriod.Custom(
    new DateTime(2024, 1, 1),
    new DateTime(2024, 3, 31),
    "Q1 2024");
```

---

## ?? Service Registration

```csharp
// In Program.cs
builder.Services.AddInvoiceDownloader(builder.Configuration);

// Get the service
var manager = host.Services.GetRequiredService<IInvoiceManager>();
```

---

## ?? Best Practices

? **Use `MetadataOnly` for reports** - Fastest, no downloads  
? **Use `PersistAndReference` for archival** - Low memory, files on disk  
? **Use `LoadInMemory` sparingly** - Only for immediate processing  
? **Configure storage in `app.json`** - Centralized configuration  
? **Use time period helpers** - `LastMonth()` instead of manual dates  
? **Check `batch.IsSuccess`** - Handle errors gracefully  

---

**Full documentation:** [USE_CASE_API_GUIDE.md](USE_CASE_API_GUIDE.md)
