# ?? Interactive CLI - User Guide

## ?? **What's New**

Your Invoice Manager now has an **interactive Spectre.Console CLI** that lets you:
- ? Choose between **classic mode** and **new high-level API**
- ? Interactively select use cases from a menu
- ? View beautiful formatted reports and statistics
- ? Evaluate different workflows side-by-side

---

## ?? **Running the Application**

```bash
cd ExtractLoadInvoices
dotnet run
```

You'll see a beautiful welcome screen and menu!

---

## ?? **Menu Options**

### **1. ?? Quick Report - Last Month (Metadata Only)**
- **What it does:** Generates a fast report without downloading files
- **Use case oriented API:** `FetchLastMonthInvoicesAsync()` with `MetadataOnly`
- **Perfect for:** Quick overview of what's available
- **Speed:** ? Fast (no downloads)

**Shows:**
- Total invoices and attachments
- Size summary
- Breakdown by vendor
- Invoice list with metadata

---

### **2. ?? Download - Last Week Invoices**
- **What it does:** Downloads last week's invoices (Monday-Sunday)
- **Use case oriented API:** `FetchLastWeekInvoicesAsync()`
- **Perfect for:** Weekly invoice processing
- **Files saved to:** Configured folders with date-sender naming

**Shows:**
- Summary statistics
- Downloaded file paths
- Processing time

---

### **3. ?? Download - Last Month Invoices**
- **What it does:** Downloads previous month's invoices
- **Use case oriented API:** `FetchLastMonthInvoicesAsync()`
- **Perfect for:** Monthly accounting reconciliation
- **Files saved to:** Configured folders

**Shows:**
- Monthly summary
- File details
- Vendor breakdown

---

### **4. ?? Download - Last Quarter Invoices**
- **What it does:** Downloads previous quarter's invoices (Q1, Q2, Q3, Q4)
- **Use case oriented API:** `FetchLastQuarterInvoicesAsync()`
- **Perfect for:** Quarterly reports, tax preparation
- **Files saved to:** Configured folders
- **Note:** Includes all emails (not just unread)

---

### **5. ?? Download - Specific Vendor**
- **What it does:** Download from a specific vendor with period selection
- **Interactive prompts:**
  1. Select vendor from configured list
  2. Select time period (Last Week, Last Month, Last Quarter, All Time)
- **Use case oriented API:** `FetchInvoicesByVendorAsync()`
- **Perfect for:** Vendor-specific audits

---

### **6. ?? Download - Custom Date Range**
- **What it does:** Download invoices within a custom date range
- **Interactive prompts:**
  1. Enter start date (yyyy-MM-dd)
  2. Enter end date (yyyy-MM-dd)
- **Use case oriented API:** `FetchInvoicesByPeriodAsync()` with `TimePeriod.Custom()`
- **Perfect for:** Tax year collection, specific period analysis

---

### **7. ?? Classic Mode - Process Configured Vendors**
- **What it does:** Runs the **original implementation**
- **Uses:** `IEmailProcessor.ProcessEmailsFromSenderAsync()`
- **Processes:** All vendors configured in `app.json`
- **Perfect for:** Maintaining backward compatibility
- **Shows:** Table with status for each vendor

---

### **8. ?? View Storage Statistics**
- **What it does:** Shows statistics about downloaded files
- **Uses:** `IAttachmentPersistenceManager.GetStatisticsAsync()`
- **Shows:**
  - Total files and size
  - Oldest and newest files
  - Files by vendor

---

### **9. ?? View Configuration**
- **What it does:** Displays current configuration
- **Shows:**
  - Google authentication settings
  - Storage configuration
  - Email mappings (sender ? folder)

---

### **10. ? Exit**
- Exits the application

---

## ?? **What You'll See**

### **Welcome Screen**
```
 ___                 _            __  __
|_ _|_ ____   _____ (_) ___ ___  |  \/  | __ _ _ __   __ _  __ _  ___ _ __
 | || '_ \ \ / / _ \| |/ __/ _ \ | |\/| |/ _` | '_ \ / _` |/ _` |/ _ \ '__|
 | || | | \ V / (_) | | (_|  __/ | |  | | (_| | | | | (_| | (_| |  __/ |
|___|_| |_|\_/ \___/|_|\___\___| |_|  |_|\__,_|_| |_|\__,_|\__, |\___|_|
                                                           |___/

Email Invoice Downloader - Choose your workflow
```

### **Menu**
```
? What would you like to do?
  ?? Quick Report - Last Month (Metadata Only)
> ?? Download - Last Week Invoices
  ?? Download - Last Month Invoices
  ?? Download - Last Quarter Invoices
  ?? Download - Specific Vendor
  ?? Download - Custom Date Range
  ?? Classic Mode - Process Configured Vendors
  ?? View Storage Statistics
  ?? View Configuration
  ? Exit
```

### **Progress Indicator**
```
? Downloading last week's invoices...
```

### **Batch Summary Report**
```
?????????????????????????????????????????
?       ?? Batch Summary                ?
?????????????????????????????????????????
? Period: Last Week (Dec 11 - Dec 17)  ?
? Total Invoices: 15                    ?
? Total Attachments: 23                 ?
? Total Size: 12.45 MB                  ?
? Processing Time: 3.21s                ?
? Status: Success                       ?
?????????????????????????????????????????

?????????????????????????????????????????
?           By Vendor                   ?
?????????????????????????????????????????
? Vendor                     ? Invoices ?
?????????????????????????????????????????
? noreply.simar@aquamatrix.. ? 15       ?
?????????????????????????????????????????

???????????????????????????????????????????????????????????
? ?? Invoice Details (showing 15 of 15)                   ?
???????????????????????????????????????????????????????????
? Date       ? Sender    ? Subject      ? Files           ?
???????????????????????????????????????????????????????????
? 2024-12-15 ? noreply.. ? Invoice #123 ? ? invoice.pdf   ?
? 2024-12-14 ? noreply.. ? Invoice #124 ? ? invoice2.pdf  ?
???????????????????????????????????????????????????????????
```

---

## ?? **Workflow Comparison**

### **Old Way (Classic Mode)**
```
Select: ?? Classic Mode - Process Configured Vendors
?
Processes all vendors from app.json
Uses original IEmailProcessor
```

### **New Way (High-Level API)**
```
Select: ?? Download - Last Month Invoices
?
Uses IInvoiceManager.FetchLastMonthInvoicesAsync()
Business-focused, time-period oriented
Rich metadata and statistics
```

---

## ?? **Tips**

### **For Quick Reports**
Use **?? Quick Report** - It's the fastest as it doesn't download files

### **For Regular Processing**
Use **?? Download - Last Week** or **?? Download - Last Month**

### **For Tax Season**
Use **?? Download - Custom Date Range** or **?? Download - Last Quarter**

### **For Debugging**
Use **?? View Configuration** to verify settings

### **For Storage Management**
Use **?? View Storage Statistics** to see what's been downloaded

---

## ?? **Example Sessions**

### **Session 1: Generate Monthly Report**
```
1. Select: ?? Quick Report - Last Month (Metadata Only)
2. Wait 2-3 seconds
3. View summary: 45 invoices, 67 attachments, 123 MB
4. Check vendor breakdown
5. Press any key to return to menu
```

### **Session 2: Download Last Week's Invoices**
```
1. Select: ?? Download - Last Week Invoices
2. Wait for download to complete
3. View downloaded files with ? checkmarks
4. Files saved to ./invoices/aguas/ with date-sender naming
5. Press any key to return to menu
```

### **Session 3: Vendor-Specific Download**
```
1. Select: ?? Download - Specific Vendor
2. Choose: noreply.simar@aquamatrix.pt
3. Select period: Last Month
4. Wait for download
5. View results
```

### **Session 4: Classic Mode**
```
1. Select: ?? Classic Mode - Process Configured Vendors
2. Watch as each vendor is processed
3. See status table with results
4. Same behavior as original implementation
```

---

## ?? **Configuration**

The CLI uses your existing `app.json`:

```json
{
  "emailsAttachmentsDestination": {
    "Values": {
      "noreply.simar@aquamatrix.pt": "aguas"
    }
  },
  "storage": {
    "defaultStorageType": "FileSystem",
    "fileSystem": {
      "baseDirectory": "./invoices",
      "defaultNamingStrategy": "WithDateAndSender"
    }
  }
}
```

---

## ?? **Troubleshooting**

### **Issue: No vendors shown in menu**
**Fix:** Add email mappings to `app.json` under `emailsAttachmentsDestination`

### **Issue: Authentication error**
**Fix:** Make sure `credentials.json` exists and is valid

### **Issue: Files not downloading**
**Fix:** Check storage configuration in `app.json`

---

## ?? **Benefits**

### **? Easy to Evaluate**
Try both classic mode and new API side-by-side

### **? Visual Feedback**
See progress spinners and formatted reports

### **? Interactive**
Choose what you need, when you need it

### **? No Code Changes**
Just run and select from menu

### **? Beautiful Output**
Formatted tables, panels, and colors

---

## ?? **Related Documentation**

- `USE_CASE_API_GUIDE.md` - Detailed API documentation
- `USE_CASE_API_QUICK_REF.md` - Quick reference
- `DATE_FILTERING_GUIDE.md` - Date filtering capabilities

---

**Enjoy your new interactive Invoice Manager! ??**
