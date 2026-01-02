namespace ExtractLoadInvoices.UseCases;

/// <summary>
/// Represents a time period for querying invoices
/// </summary>
public class TimePeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public static TimePeriod LastWeek() 
    {
        var today = DateTime.Today;
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var lastMonday = today.AddDays(-daysSinceMonday - 7);
        var lastSunday = lastMonday.AddDays(6);
        
        return new TimePeriod 
        { 
            StartDate = lastMonday, 
            EndDate = lastSunday.AddDays(1),
            Description = $"Last Week ({lastMonday:MMM dd} - {lastSunday:MMM dd, yyyy})"
        };
    }
    
    public static TimePeriod ThisWeek()
    {
        var today = DateTime.Today;
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = today.AddDays(-daysSinceMonday);
        var nextMonday = monday.AddDays(7);
        
        return new TimePeriod 
        { 
            StartDate = monday, 
            EndDate = nextMonday,
            Description = $"This Week ({monday:MMM dd} - {nextMonday.AddDays(-1):MMM dd, yyyy})"
        };
    }
    
    public static TimePeriod LastMonth()
    {
        var firstDayOfCurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var firstDayOfLastMonth = firstDayOfCurrentMonth.AddMonths(-1);
        
        return new TimePeriod 
        { 
            StartDate = firstDayOfLastMonth, 
            EndDate = firstDayOfCurrentMonth,
            Description = $"Last Month ({firstDayOfLastMonth:MMMM yyyy})"
        };
    }
    
    public static TimePeriod ThisMonth()
    {
        var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = firstDay.AddMonths(1);
        
        return new TimePeriod 
        { 
            StartDate = firstDay, 
            EndDate = nextMonth,
            Description = $"This Month ({firstDay:MMMM yyyy})"
        };
    }
    
    public static TimePeriod LastQuarter()
    {
        var today = DateTime.Today;
        var currentQuarter = (today.Month - 1) / 3;
        var lastQuarter = currentQuarter - 1;
        var year = today.Year;
        
        if (lastQuarter < 0)
        {
            lastQuarter = 3;
            year--;
        }
        
        var startMonth = lastQuarter * 3 + 1;
        var start = new DateTime(year, startMonth, 1);
        var end = start.AddMonths(3);
        
        return new TimePeriod 
        { 
            StartDate = start, 
            EndDate = end,
            Description = $"Q{lastQuarter + 1} {year}"
        };
    }
    
    public static TimePeriod ThisQuarter()
    {
        var today = DateTime.Today;
        var currentQuarter = (today.Month - 1) / 3;
        var startMonth = currentQuarter * 3 + 1;
        var start = new DateTime(today.Year, startMonth, 1);
        var end = start.AddMonths(3);
        
        return new TimePeriod 
        { 
            StartDate = start, 
            EndDate = end,
            Description = $"Q{currentQuarter + 1} {today.Year}"
        };
    }
    
    public static TimePeriod LastYear()
    {
        var lastYear = DateTime.Today.Year - 1;
        
        return new TimePeriod 
        { 
            StartDate = new DateTime(lastYear, 1, 1), 
            EndDate = new DateTime(lastYear + 1, 1, 1),
            Description = $"Year {lastYear}"
        };
    }
    
    public static TimePeriod ThisYear()
    {
        var thisYear = DateTime.Today.Year;
        
        return new TimePeriod 
        { 
            StartDate = new DateTime(thisYear, 1, 1), 
            EndDate = new DateTime(thisYear + 1, 1, 1),
            Description = $"Year {thisYear}"
        };
    }
    
    public static TimePeriod Custom(DateTime start, DateTime end, string? description = null)
    {
        return new TimePeriod 
        { 
            StartDate = start, 
            EndDate = end,
            Description = description ?? $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd}"
        };
    }
    
    public static TimePeriod LastNDays(int days)
    {
        var end = DateTime.Today.AddDays(1);
        var start = end.AddDays(-days);
        
        return new TimePeriod
        {
            StartDate = start,
            EndDate = end,
            Description = $"Last {days} days"
        };
    }
}
