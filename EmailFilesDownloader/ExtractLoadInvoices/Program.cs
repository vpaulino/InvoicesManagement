// See https://aka.ms/new-console-template for more information
using ExtractLoadInvoices;
using ExtractLoadInvoices.Configuration;
using ExtractLoadInvoices.Services;
using ExtractLoadInvoices.Storage;
using ExtractLoadInvoices.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);

// Configure configuration sources
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "app.json"), optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Register all invoice downloader services
builder.Services.AddInvoiceDownloader(builder.Configuration);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// Display welcome banner
AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("Invoice Manager")
        .LeftJustified()
        .Color(Color.Blue));

AnsiConsole.MarkupLine("[dim]Email Invoice Downloader - Choose your workflow[/]\n");

// Main menu loop
var running = true;
while (running)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]What would you like to do?[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices(new[]
            {
                "📊 Quick Report - Last Month (Metadata Only)",
                "📥 Download - Last Week Invoices",
                "📅 Download - Last Month Invoices",
                "📈 Download - Last Quarter Invoices",
                "🎯 Download - Specific Vendor",
                "📆 Download - Custom Date Range",
                "🔧 Classic Mode - Process Configured Vendors",
                "📁 View Storage Statistics",
                "⚙️ View Configuration",
                "❌ Exit"
            }));

    try
    {
        switch (choice)
        {
            case "📊 Quick Report - Last Month (Metadata Only)":
                await RunQuickReportAsync(host);
                break;
            
            case "📥 Download - Last Week Invoices":
                await RunDownloadLastWeekAsync(host);
                break;
            
            case "📅 Download - Last Month Invoices":
                await RunDownloadLastMonthAsync(host);
                break;
            
            case "📈 Download - Last Quarter Invoices":
                await RunDownloadLastQuarterAsync(host);
                break;
            
            case "🎯 Download - Specific Vendor":
                await RunDownloadByVendorAsync(host);
                break;
            
            case "📆 Download - Custom Date Range":
                await RunCustomDateRangeAsync(host);
                break;
            
            case "🔧 Classic Mode - Process Configured Vendors":
                await RunClassicModeAsync(host);
                break;
            
            case "📁 View Storage Statistics":
                await ViewStorageStatsAsync(host);
                break;
            
            case "⚙️ View Configuration":
                await ViewConfigurationAsync(host);
                break;
            
            case "❌ Exit":
                running = false;
                break;
        }
        
        if (running)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        AnsiConsole.WriteException(ex);
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
        AnsiConsole.Clear();
    }
}

AnsiConsole.MarkupLine("\n[blue]Thank you for using Invoice Manager![/]");
return 0;

// ==================== USE CASE IMPLEMENTATIONS ====================

async Task RunQuickReportAsync(IHost host)
{
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Fetching invoice metadata...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var batch = await manager.FetchLastMonthEmailFilesAsync(new FetchOptions
            {
                IncludeMetadata = true,
                IncludeEmailBody = false,
                IncludeAttachments = true,
                AttachmentStrategy = AttachmentHandlingStrategy.MetadataOnly
            });
            
            ctx.Status("Generating report...");
            
            DisplayBatchReport(batch);
        });
}

async Task RunDownloadLastWeekAsync(IHost host)
{
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Downloading last week's invoices...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var batch = await manager.FetchLastWeekEmailFilesAsync(new FetchOptions
            {
                IncludeMetadata = true,
                AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
                NamingStrategy = FileNamingStrategy.WithDateAndSender
            });
            
            DisplayBatchReport(batch, showFiles: true);
        });
}

async Task RunDownloadLastMonthAsync(IHost host)
{
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Downloading last month's invoices...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var batch = await manager.FetchLastMonthEmailFilesAsync(new FetchOptions
            {
                IncludeMetadata = true,
                AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
                NamingStrategy = FileNamingStrategy.WithDateAndSender
            });
            
            DisplayBatchReport(batch, showFiles: true);
        });
}

async Task RunDownloadLastQuarterAsync(IHost host)
{
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Downloading last quarter's invoices...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var batch = await manager.FetchLastQuarterEmailFilesAsync(new FetchOptions
            {
                IncludeMetadata = true,
                AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
                NamingStrategy = FileNamingStrategy.WithDateAndSender,
                UnreadOnly = false
            });
            
            DisplayBatchReport(batch, showFiles: true);
        });
}

async Task RunDownloadByVendorAsync(IHost host)
{
    var configService = host.Services.GetRequiredService<IConfigurationService>();
    var settings = configService.LoadConfiguration();
    
    var vendors = settings.EmailMappings.SenderToFolderMap.Keys.ToList();
    
    if (vendors.Count == 0)
    {
        AnsiConsole.MarkupLine("[red]No vendors configured in app.json[/]");
        return;
    }
    
    var selectedVendor = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select a [green]vendor[/]:")
            .AddChoices(vendors));
    
    var period = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select [green]time period[/]:")
            .AddChoices(new[]
            {
                "Last Week",
                "Last Month",
                "Last Quarter",
                "All Time"
            }));
    
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    TimePeriod? selectedPeriod = period switch
    {
        "Last Week" => TimePeriod.LastWeek(),
        "Last Month" => TimePeriod.LastMonth(),
        "Last Quarter" => TimePeriod.LastQuarter(),
        _ => null
    };
    
    await AnsiConsole.Status()
        .StartAsync($"Downloading invoices from {selectedVendor}...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var batch = await manager.FetchEmailFilesByVendorAsync(
                selectedVendor,
                selectedPeriod,
                new FetchOptions
                {
                    IncludeMetadata = true,
                    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
                    NamingStrategy = FileNamingStrategy.WithDateAndSender,
                    UnreadOnly = false
                });
            
            DisplayBatchReport(batch, showFiles: true);
        });
}

async Task RunCustomDateRangeAsync(IHost host)
{
    AnsiConsole.MarkupLine("[yellow]Enter date range (dates are inclusive)[/]");
    
    var startDate = AnsiConsole.Ask<DateTime>("Start date [grey](yyyy-MM-dd)[/]:");
    var endDate = AnsiConsole.Ask<DateTime>("End date [grey](yyyy-MM-dd)[/]:");
    
    if (endDate < startDate)
    {
        AnsiConsole.MarkupLine("[red]End date must be after start date![/]");
        return;
    }
    
    var manager = host.Services.GetRequiredService<IEmailFilesManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Downloading invoices for custom date range...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            
            var period = TimePeriod.Custom(startDate, endDate.AddDays(1));
            
            var batch = await manager.FetchEmailFilesByPeriodAsync(
                period,
                new FetchOptions
                {
                    IncludeMetadata = true,
                    AttachmentStrategy = AttachmentHandlingStrategy.PersistAndReference,
                    NamingStrategy = FileNamingStrategy.WithDateAndSender,
                    UnreadOnly = false
                });
            
            DisplayBatchReport(batch, showFiles: true);
        });
}

async Task RunClassicModeAsync(IHost host)
{
    AnsiConsole.MarkupLine("[yellow]Running in Classic Mode - Processing all configured vendors[/]");
    
    var processor = host.Services.GetRequiredService<IEmailProcessor>();
    var configService = host.Services.GetRequiredService<IConfigurationService>();
    var settings = configService.LoadConfiguration();
    
    var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("[green]Vendor[/]")
        .AddColumn("[blue]Folder[/]")
        .AddColumn("[yellow]Status[/]");
    
    foreach (var mapping in settings.EmailMappings.SenderToFolderMap)
    {
        await AnsiConsole.Status()
            .StartAsync($"Processing {mapping.Key}...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                
                var result = await processor.ProcessEmailsFromSenderAsync(mapping.Key, mapping.Value);
                
                var status = result.IsSuccess
                    ? $"[green]✓ {result.EmailsProcessed} emails, {result.AttachmentsDownloaded} files[/]"
                    : $"[red]✗ {result.Errors.Count} errors[/]";
                
                table.AddRow(mapping.Key, mapping.Value, status);
            });
    }
    
    AnsiConsole.Write(table);
}

async Task ViewStorageStatsAsync(IHost host)
{
    var storage = host.Services.GetRequiredService<IAttachmentPersistenceManager>();
    
    await AnsiConsole.Status()
        .StartAsync("Calculating storage statistics...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            
            var stats = await storage.GetStatisticsAsync();
            
            var panel = new Panel(
                new Rows(
                    new Markup($"[bold]Total Files:[/] {stats.TotalFiles}"),
                    new Markup($"[bold]Total Size:[/] {stats.TotalSizeBytes / 1024.0 / 1024.0:F2} MB"),
                    new Markup($"[bold]Oldest File:[/] {stats.OldestFile?.ToString("yyyy-MM-dd") ?? "N/A"}"),
                    new Markup($"[bold]Newest File:[/] {stats.NewestFile?.ToString("yyyy-MM-dd") ?? "N/A"}")
                ))
            {
                Header = new PanelHeader("[yellow]Storage Statistics[/]"),
                Border = BoxBorder.Rounded
            };
            
            AnsiConsole.Write(panel);
            
            if (stats.FilesByVendor.Count > 0)
            {
                AnsiConsole.WriteLine();
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("[green]Vendor[/]")
                    .AddColumn("[blue]Files[/]");
                
                foreach (var vendor in stats.FilesByVendor.OrderByDescending(x => x.Value))
                {
                    table.AddRow(vendor.Key, vendor.Value.ToString());
                }
                
                AnsiConsole.Write(table);
            }
        });
}

async Task ViewConfigurationAsync(IHost host)
{
    var configService = host.Services.GetRequiredService<IConfigurationService>();
    var settings = configService.LoadConfiguration();
    
    var grid = new Grid()
        .AddColumn()
        .AddRow(new Panel(
            new Markup($"[bold]Application:[/] {settings.ApplicationName}\n" +
                      $"[bold]Credentials:[/] {settings.GoogleCredentials.CredentialsLocation}\n" +
                      $"[bold]Token:[/] {settings.GoogleCredentials.TokenDestination}"))
            .Header("[yellow]Google Configuration[/]")
            .Border(BoxBorder.Rounded))
        .AddRow(new Markup(""))
        .AddRow(new Panel(
            new Markup($"[bold]Storage Type:[/] {settings.Storage.DefaultStorageType}\n" +
                      $"[bold]Base Directory:[/] {settings.Storage.FileSystem?.BaseDirectory ?? "N/A"}\n" +
                      $"[bold]Naming Strategy:[/] {settings.Storage.FileSystem?.DefaultNamingStrategy.ToString() ?? "N/A"}"))
            .Header("[yellow]Storage Configuration[/]")
            .Border(BoxBorder.Rounded));
    
    AnsiConsole.Write(grid);
    
    if (settings.EmailMappings.SenderToFolderMap.Count > 0)
    {
        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[green]Sender Email[/]")
            .AddColumn("[blue]Destination Folder[/]");
        
        foreach (var mapping in settings.EmailMappings.SenderToFolderMap)
        {
            table.AddRow(mapping.Key, mapping.Value);
        }
        
        AnsiConsole.Write(new Panel(table)
            .Header("[yellow]Email Mappings[/]")
            .Border(BoxBorder.Rounded));
    }
    
    await Task.CompletedTask;
}

void DisplayBatchReport(EmailFilesBatch batch, bool showFiles = false)
{
    var panel = new Panel(
        new Rows(
            new Markup($"[bold]Period:[/] {batch.Metadata.Period?.Description ?? "N/A"}"),
            new Markup($"[bold]Total Invoices:[/] {batch.Metadata.TotalInvoices}"),
            new Markup($"[bold]Total Attachments:[/] {batch.Metadata.TotalAttachments}"),
            new Markup($"[bold]Total Size:[/] {batch.Metadata.TotalSizeBytes / 1024.0 / 1024.0:F2} MB"),
            new Markup($"[bold]Processing Time:[/] {batch.Metadata.ProcessingTime.TotalSeconds:F2}s"),
            new Markup($"[bold]Status:[/] {(batch.IsSuccess ? "[green]Success[/]" : "[red]Errors: " + batch.Errors.Count + "[/]")}")
        ))
    {
        Header = new PanelHeader("[green]📊 Batch Summary[/]"),
        Border = BoxBorder.Double
    };
    
    AnsiConsole.Write(panel);
    
    if (batch.Metadata.InvoicesByVendor.Count > 0)
    {
        AnsiConsole.WriteLine();
        var vendorTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[green]Vendor[/]")
            .AddColumn("[blue]Invoices[/]");
        
        foreach (var vendor in batch.Metadata.InvoicesByVendor.OrderByDescending(x => x.Value))
        {
            vendorTable.AddRow(vendor.Key, vendor.Value.ToString());
        }
        
        AnsiConsole.Write(new Panel(vendorTable)
            .Header("[yellow]By Vendor[/]")
            .Border(BoxBorder.Rounded));
    }
    
    if (showFiles && batch.EmailAttachments.Count > 0)
    {
        AnsiConsole.WriteLine();
        
        var fileTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[green]Date[/]")
            .AddColumn("[blue]Sender[/]")
            .AddColumn("[yellow]Subject[/]")
            .AddColumn("[cyan]Files[/]");
        
        foreach (var invoice in batch.EmailAttachments.Take(20))
        {
            var fileCount = invoice.Attachments.Count;
            var fileInfo = fileCount > 0
                ? string.Join("\n", invoice.Attachments.Take(3).Select(a => 
                    a.IsPersisted 
                        ? $"✓ {Path.GetFileName(a.StorageReference)}" 
                        : $"• {a.FileName}"))
                : "No files";
            
            fileTable.AddRow(
                invoice.SentDate.ToString("yyyy-MM-dd"),
                invoice.Sender.Length > 30 ? invoice.Sender.Substring(0, 27) + "..." : invoice.Sender,
                invoice.Subject.Length > 40 ? invoice.Subject.Substring(0, 37) + "..." : invoice.Subject,
                fileInfo);
        }
        
        if (batch.EmailAttachments.Count > 20)
        {
            fileTable.AddRow(
                "[dim]...[/]",
                $"[dim]... and {batch.EmailAttachments.Count - 20} more[/]",
                "[dim]...[/]",
                "[dim]...[/]");
        }
        
        AnsiConsole.Write(new Panel(fileTable)
            .Header($"[yellow]📎 Invoice Details (showing {Math.Min(20, batch.EmailAttachments.Count)} of {batch.EmailAttachments.Count})[/]")
            .Border(BoxBorder.Rounded));
    }
    
    if (!batch.IsSuccess)
    {
        AnsiConsole.WriteLine();
        var errorList = string.Join("\n", batch.Errors.Select(e => $"• {Markup.Escape(e)}"));
        var errorPanel = new Panel(new Markup($"[red]{errorList}[/]"))
        {
            Header = new PanelHeader("[red]⚠ Errors[/]"),
            Border = BoxBorder.Rounded
        };
        AnsiConsole.Write(errorPanel);
    }
}