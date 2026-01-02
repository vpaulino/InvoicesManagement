# ?? Date Filtering Quick Reference

## Quick Examples

### Last 7 Days
```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    NewerThan = 7
};
```

### Specific Date Range
```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    After = new DateTime(2024, 1, 1),
    Before = new DateTime(2024, 1, 31)
};
```

### Specific Day
```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    On = new DateTime(2024, 12, 15)
};
```

### Older Than 30 Days
```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    OlderThan = 30,
    UnreadOnly = false
};
```

### This Month
```csharp
var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
var nextMonth = firstDay.AddMonths(1);

var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    After = firstDay,
    Before = nextMonth
};
```

## Available Properties

| Property | Type | Description |
|----------|------|-------------|
| `After` | `DateTime?` | Emails after this date (inclusive) |
| `Before` | `DateTime?` | Emails before this date (exclusive) |
| `On` | `DateTime?` | Emails on specific date |
| `NewerThan` | `int?` | Emails from last N days |
| `OlderThan` | `int?` | Emails older than N days |

## Gmail Query Output

| Code | Gmail Query |
|------|-------------|
| `NewerThan = 7` | `newer_than:7d` |
| `OlderThan = 30` | `older_than:30d` |
| `After = new DateTime(2024,1,1)` | `after:2024/01/01` |
| `Before = new DateTime(2024,12,31)` | `before:2024/12/31` |
| `On = new DateTime(2024,6,15)` | `after:2024/06/15 before:2024/06/16` |

## Full Documentation
See [DATE_FILTERING_GUIDE.md](DATE_FILTERING_GUIDE.md) for complete documentation.
