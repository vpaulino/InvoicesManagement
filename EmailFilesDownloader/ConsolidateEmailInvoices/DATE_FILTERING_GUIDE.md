# ?? Date Filtering Guide for EmailQuery

## Overview

The `EmailQuery` class now supports comprehensive date-based filtering using Gmail's search operators. You can filter emails by date ranges, specific dates, or relative time periods.

---

## ?? Date Filter Properties

### **Absolute Date Filters**

| Property | Type | Description | Gmail Operator |
|----------|------|-------------|----------------|
| `After` | `DateTime?` | Emails received **after** this date (inclusive) | `after:yyyy/MM/dd` |
| `Before` | `DateTime?` | Emails received **before** this date (exclusive) | `before:yyyy/MM/dd` |
| `On` | `DateTime?` | Emails received **on** a specific date | `after:yyyy/MM/dd before:yyyy/MM/dd+1` |

### **Relative Date Filters**

| Property | Type | Description | Gmail Operator |
|----------|------|-------------|----------------|
| `NewerThan` | `int?` | Emails from last N days | `newer_than:Nd` |
| `OlderThan` | `int?` | Emails older than N days | `older_than:Nd` |

---

## ?? Usage Examples

### **Example 1: Get Invoices from Last 7 Days**

```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    NewerThan = 7  // Last 7 days
};

var emails = await gmailService.GetEmailsAsync(query);
```

**Gmail Query:** `from:invoices@company.com newer_than:7d`

---

### **Example 2: Get Invoices Between Two Dates**

```csharp
var query = new EmailQuery
{
    SenderEmail = "billing@vendor.com",
    After = new DateTime(2024, 1, 1),
    Before = new DateTime(2024, 1, 31),
    UnreadOnly = false  // Include read emails
};

var emails = await gmailService.GetEmailsAsync(query);
```

**Gmail Query:** `from:billing@vendor.com after:2024/01/01 before:2024/01/31`

---

### **Example 3: Get Invoices from a Specific Date**

```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    On = new DateTime(2024, 12, 15)
};

var emails = await gmailService.GetEmailsAsync(query);
```

**Gmail Query:** `from:invoices@company.com after:2024/12/15 before:2024/12/16`

---

### **Example 4: Get Unread Invoices from Last Month**

```csharp
var query = new EmailQuery
{
    SenderEmail = "billing@service.com",
    After = DateTime.Now.AddMonths(-1),
    UnreadOnly = true
};

var emails = await gmailService.GetEmailsAsync(query);
```

**Gmail Query:** `from:billing@service.com in:unread after:2024/11/15`

---

### **Example 5: Get Old Invoices (Older than 30 Days)**

```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    OlderThan = 30,
    UnreadOnly = false
};

var emails = await gmailService.GetEmailsAsync(query);
```

**Gmail Query:** `from:invoices@company.com older_than:30d`

---

### **Example 6: Archive Old Processed Invoices**

```csharp
// Get invoices older than 90 days that were already marked as read
var query = new EmailQuery
{
    SenderEmail = "billing@vendor.com",
    OlderThan = 90,
    UnreadOnly = false
};

var oldEmails = await gmailService.GetEmailsAsync(query);

// Process or archive them...
```

---

### **Example 7: Download This Month's Invoices Only**

```csharp
var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    After = firstDayOfMonth,
    Before = firstDayOfNextMonth
};

var result = await emailProcessor.ProcessEmailsFromSenderAsync(
    query.SenderEmail, 
    "current-month-invoices");
```

---

### **Example 8: Download Last Quarter's Invoices**

```csharp
var today = DateTime.Now;
var quarterStart = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1).AddMonths(-3);
var quarterEnd = quarterStart.AddMonths(3);

var query = new EmailQuery
{
    SenderEmail = "billing@company.com",
    After = quarterStart,
    Before = quarterEnd,
    UnreadOnly = false
};

var emails = await gmailService.GetEmailsAsync(query);
```

---

## ?? Integration with EmailProcessor

You can now extend `EmailProcessor` to support date filtering:

### **Option 1: Add Overload Method**

```csharp
public interface IEmailProcessor
{
    Task<ProcessingResult> ProcessEmailsFromSenderAsync(
        string senderEmail, 
        string destinationFolder);
    
    // New overload with date filtering
    Task<ProcessingResult> ProcessEmailsAsync(
        EmailQuery query, 
        string destinationFolder);
}
```

### **Option 2: Use Existing Method with Custom Query**

Modify the current `EmailProcessor` to accept `EmailQuery` directly:

```csharp
// In your custom code
var customQuery = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    After = DateTime.Now.AddDays(-30),
    UnreadOnly = true
};

// Use GmailService directly
var gmailService = serviceProvider.GetRequiredService<IGmailService>();
var emails = await gmailService.GetEmailsAsync(customQuery);

// Process manually or extend EmailProcessor
```

---

## ?? Advanced Filtering Scenarios

### **Scenario 1: Monthly Invoice Download Job**

```csharp
public class MonthlyInvoiceDownloader
{
    private readonly IEmailProcessor _processor;
    private readonly IGmailService _gmailService;

    public async Task DownloadMonthlyInvoicesAsync(int year, int month, string sender)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var query = new EmailQuery
        {
            SenderEmail = sender,
            After = startDate,
            Before = endDate,
            UnreadOnly = false
        };

        var emails = await _gmailService.GetEmailsAsync(query);
        
        // Process emails...
    }
}
```

---

### **Scenario 2: Year-End Tax Document Collection**

```csharp
public async Task DownloadTaxYearInvoicesAsync(int taxYear)
{
    var yearStart = new DateTime(taxYear, 1, 1);
    var yearEnd = new DateTime(taxYear + 1, 1, 1);

    var senders = new[] { "billing@vendor1.com", "invoices@vendor2.com" };

    foreach (var sender in senders)
    {
        var query = new EmailQuery
        {
            SenderEmail = sender,
            After = yearStart,
            Before = yearEnd,
            UnreadOnly = false
        };

        await _processor.ProcessEmailsAsync(query, $"tax-{taxYear}");
    }
}
```

---

### **Scenario 3: Daily Invoice Automation**

```csharp
public class DailyInvoiceService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var query = new EmailQuery
            {
                SenderEmail = "daily-invoices@company.com",
                NewerThan = 1,  // Last 24 hours
                UnreadOnly = true
            };

            var result = await _processor.ProcessEmailsAsync(query, "daily-invoices");
            
            _logger.LogInformation("Downloaded {Count} invoices", result.AttachmentsDownloaded);

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

---

## ?? Gmail Search Operator Reference

The `BuildQueryString()` method generates Gmail search syntax:

| Filter | Example Code | Generated Query |
|--------|-------------|-----------------|
| **After Date** | `After = new DateTime(2024, 1, 1)` | `after:2024/01/01` |
| **Before Date** | `Before = new DateTime(2024, 12, 31)` | `before:2024/12/31` |
| **Specific Date** | `On = new DateTime(2024, 6, 15)` | `after:2024/06/15 before:2024/06/16` |
| **Last N Days** | `NewerThan = 7` | `newer_than:7d` |
| **Older Than N Days** | `OlderThan = 30` | `older_than:30d` |

---

## ?? Important Notes

### **Date Precedence Rules**

1. **`On` takes precedence** over `After` and `Before`
2. **Relative filters** (`NewerThan`, `OlderThan`) can be combined with absolute dates
3. All date filters work together with `SenderEmail` and `UnreadOnly`

### **Date Format**

- Gmail expects dates in **yyyy/MM/dd** format
- The query builder handles this automatically
- Dates are in **UTC** timezone

### **Performance Considerations**

- **Narrower date ranges = faster queries**
- Use `NewerThan` for recent emails (more efficient)
- Combine with `UnreadOnly = true` to reduce result sets

---

## ?? Testing Your Queries

You can test the generated query string:

```csharp
var query = new EmailQuery
{
    SenderEmail = "test@example.com",
    After = new DateTime(2024, 1, 1),
    Before = new DateTime(2024, 12, 31),
    UnreadOnly = true
};

Console.WriteLine(query.BuildQueryString());
// Output: from:test@example.com in:unread after:2024/01/01 before:2024/12/31
```

You can also test this query directly in Gmail's web interface search box!

---

## ?? Summary

The enhanced `EmailQuery` now supports:

? **Absolute dates** - Filter by specific date ranges  
? **Relative dates** - Filter by "last N days" or "older than N days"  
? **Specific dates** - Filter emails from a single day  
? **Flexible combinations** - Mix date filters with sender and read/unread filters  
? **Gmail syntax** - Automatically generates proper Gmail search queries  

**This makes it easy to:**
- Download monthly invoices
- Archive old emails
- Process only recent invoices
- Generate tax year reports
- Build scheduled automation tasks
