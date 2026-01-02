# Usage Examples

## Example 1: Using in a Console Application

```csharp
using ExtractLoadInvoices;
using ExtractLoadInvoices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("app.json");

// Register invoice downloader services
builder.Services.AddInvoiceDownloader(builder.Configuration);

// Add logging
builder.Logging.AddConsole();

var host = builder.Build();

// Use the email processor
var processor = host.Services.GetRequiredService<IEmailProcessor>();

var result = await processor.ProcessEmailsFromSenderAsync(
    "invoices@company.com", 
    "my-invoices");

Console.WriteLine($"Processed {result.EmailsProcessed} emails");
Console.WriteLine($"Downloaded {result.AttachmentsDownloaded} attachments");

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

## Example 2: Using in ASP.NET Core Web API

```csharp
// In Program.cs or Startup.cs
using ExtractLoadInvoices;

var builder = WebApplication.CreateBuilder(args);

// Add invoice downloader services
builder.Services.AddInvoiceDownloader(builder.Configuration);

var app = builder.Build();

// Create an endpoint to trigger invoice download
app.MapPost("/api/invoices/download", async (
    IEmailProcessor processor, 
    string senderEmail, 
    string destinationFolder) =>
{
    var result = await processor.ProcessEmailsFromSenderAsync(
        senderEmail, 
        destinationFolder);
    
    return Results.Ok(new 
    { 
        success = result.IsSuccess,
        emailsProcessed = result.EmailsProcessed,
        attachmentsDownloaded = result.AttachmentsDownloaded,
        errors = result.Errors
    });
});

app.Run();
```

## Example 3: Using Individual Services with Date Filtering

```csharp
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Models;
using Microsoft.Extensions.DependencyInjection;

// Get specific services
var gmailService = serviceProvider.GetRequiredService<IGmailService>();

// Query emails from last 7 days
var query = new EmailQuery
{
    SenderEmail = "billing@vendor.com",
    NewerThan = 7,  // Last 7 days
    UnreadOnly = true
};

var emails = await gmailService.GetEmailsAsync(query);

foreach (var email in emails)
{
    var details = await gmailService.GetMessageDetailsAsync(email.Id);
    Console.WriteLine($"Email: {email.Id}");
    
    // Mark as read
    await gmailService.MarkAsReadAsync(email.Id);
}
```

## Example 4: Download Invoices by Date Range

```csharp
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Models;

var gmailService = serviceProvider.GetRequiredService<IGmailService>();

// Download invoices from January 2024
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    After = new DateTime(2024, 1, 1),
    Before = new DateTime(2024, 2, 1),
    UnreadOnly = false  // Include both read and unread
};

var emails = await gmailService.GetEmailsAsync(query);

Console.WriteLine($"Found {emails.Count()} invoices from January 2024");
```

## Example 5: Monthly Invoice Archive

```csharp
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Models;

public async Task ArchiveMonthlyInvoices(int year, int month)
{
    var gmailService = serviceProvider.GetRequiredService<IGmailService>();
    var processor = serviceProvider.GetRequiredService<IEmailProcessor>();

    var startDate = new DateTime(year, month, 1);
    var endDate = startDate.AddMonths(1);

    var query = new EmailQuery
    {
        SenderEmail = "billing@vendor.com",
        After = startDate,
        Before = endDate,
        UnreadOnly = false
    };

    var emails = await gmailService.GetEmailsAsync(query);
    
    foreach (var email in emails)
    {
        // Process each email...
        Console.WriteLine($"Archiving email from {startDate:yyyy-MM}");
    }
}
```

## Example 6: Custom Attachment Processing

```csharp
using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.Services;

var gmailService = serviceProvider.GetRequiredService<IGmailService>();
var attachmentDownloader = serviceProvider.GetRequiredService<IAttachmentDownloader>();

// Get a message
var message = await gmailService.GetMessageDetailsAsync("message-id-123");

// Process attachments manually
if (message.Payload?.Parts != null)
{
    foreach (var part in message.Payload.Parts)
    {
        if (!string.IsNullOrEmpty(part.Filename) && part.MimeType == "application/pdf")
        {
            var attachment = await attachmentDownloader.DownloadAttachmentAsync(
                message.Id,
                part.Body.AttachmentId,
                part.Filename,
                part.MimeType);
            
            // Do something with attachment.Data
            Console.WriteLine($"Downloaded: {attachment.FileName} ({attachment.Data.Length} bytes)");
        }
    }
}
```

## Example 7: Scheduled Background Service with Date Filtering

```csharp
using ExtractLoadInvoices.Configuration;
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Models;
using Microsoft.Extensions.Hosting;

public class InvoiceDownloadBackgroundService : BackgroundService
{
    private readonly IGmailService _gmailService;
    private readonly IConfigurationService _configService;
    private readonly ILogger<InvoiceDownloadBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public InvoiceDownloadBackgroundService(
        IGmailService gmailService,
        IConfigurationService configService,
        ILogger<InvoiceDownloadBackgroundService> logger)
    {
        _gmailService = gmailService;
        _configService = configService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting scheduled invoice download");
            
            var settings = _configService.LoadConfiguration();

            foreach (var mapping in settings.EmailMappings.SenderToFolderMap)
            {
                // Only get emails from last 24 hours
                var query = new EmailQuery
                {
                    SenderEmail = mapping.Key,
                    NewerThan = 1,  // Last 24 hours
                    UnreadOnly = true
                };
                
                var emails = await _gmailService.GetEmailsAsync(query);
                
                _logger.LogInformation(
                    "Found {EmailCount} new emails from {Sender}",
                    emails.Count(), 
                    mapping.Key);
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}

// Register in Program.cs
builder.Services.AddHostedService<InvoiceDownloadBackgroundService>();
```

## Example 8: Unit Testing

```csharp
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Attachments;
using ExtractLoadInvoices.FileSystem;
using ExtractLoadInvoices.Models;
using Moq;
using Xunit;

public class EmailProcessorTests
{
    [Fact]
    public async Task ProcessEmailsFromSenderAsync_WithValidEmails_DownloadsAttachments()
    {
        // Arrange
        var mockGmailService = new Mock<IGmailService>();
        var mockAttachmentFilter = new Mock<IAttachmentFilter>();
        var mockDownloader = new Mock<IAttachmentDownloader>();
        var mockFileStorage = new Mock<IFileStorageService>();
        var mockLogger = new Mock<ILogger<EmailProcessor>>();

        var messages = new List<Message>
        {
            new Message { Id = "msg-1" }
        };

        mockGmailService
            .Setup(x => x.GetEmailsAsync(It.IsAny<EmailQuery>()))
            .ReturnsAsync(messages);

        var processor = new EmailProcessor(
            mockGmailService.Object,
            mockAttachmentFilter.Object,
            mockDownloader.Object,
            mockFileStorage.Object,
            mockLogger.Object);

        // Act
        var result = await processor.ProcessEmailsFromSenderAsync(
            "test@example.com", 
            "test-folder");

        // Assert
        Assert.True(result.EmailsProcessed > 0);
    }
}
```

## Example 9: Year-End Tax Document Collection

```csharp
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Models;

public async Task DownloadTaxYearInvoices(int taxYear)
{
    var gmailService = serviceProvider.GetRequiredService<IGmailService>();
    
    var yearStart = new DateTime(taxYear, 1, 1);
    var yearEnd = new DateTime(taxYear + 1, 1, 1);

    var vendors = new[] 
    { 
        "billing@vendor1.com", 
        "invoices@vendor2.com",
        "accounting@supplier.com"
    };

    foreach (var vendor in vendors)
    {
        var query = new EmailQuery
        {
            SenderEmail = vendor,
            After = yearStart,
            Before = yearEnd,
            UnreadOnly = false  // Include all emails
        };

        var emails = await gmailService.GetEmailsAsync(query);
        
        Console.WriteLine($"Found {emails.Count()} invoices from {vendor} for tax year {taxYear}");
    }
}
```

## Example 10: Custom Configuration

```csharp
using ExtractLoadInvoices.Configuration;

// Load custom configuration
var customConfig = new AppSettings
{
    ApplicationName = "MyInvoiceApp",
    GoogleCredentials = new GoogleCredentialsSettings
    {
        CredentialsLocation = "path/to/credentials.json",
        TokenDestination = "path/to/token.json"
    },
    EmailMappings = new EmailMappingSettings
    {
        SenderToFolderMap = new Dictionary<string, string>
        {
            { "billing@company1.com", "company1-invoices" },
            { "invoices@company2.com", "company2-invoices" }
        }
    }
};

// Use with services...
```

---

## ?? More Date Filtering Examples

See [DATE_FILTERING_GUIDE.md](DATE_FILTERING_GUIDE.md) for comprehensive date filtering documentation including:
- Absolute date filtering (After, Before, On)
- Relative date filtering (NewerThan, OlderThan)
- Monthly/quarterly invoice downloads
- Advanced scenarios and use cases
