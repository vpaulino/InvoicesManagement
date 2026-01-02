namespace ExtractLoadInvoices.Models;

public class EmailQuery
{
    public string SenderEmail { get; set; } = string.Empty;
    public bool UnreadOnly { get; set; } = true;
    public bool IncludeSpamTrash { get; set; } = false;
    public string LabelId { get; set; } = "INBOX";
    
    /// <summary>
    /// Filter emails received after this date (inclusive)
    /// Format: yyyy/MM/dd
    /// </summary>
    public DateTime? After { get; set; }
    
    /// <summary>
    /// Filter emails received before this date (exclusive)
    /// Format: yyyy/MM/dd
    /// </summary>
    public DateTime? Before { get; set; }
    
    /// <summary>
    /// Filter emails within a specific date (same as After and Before on same date)
    /// Format: yyyy/MM/dd
    /// </summary>
    public DateTime? On { get; set; }
    
    /// <summary>
    /// Filter emails older than specified number of days
    /// Example: OlderThan = 7 means emails older than 7 days
    /// </summary>
    public int? OlderThan { get; set; }
    
    /// <summary>
    /// Filter emails newer than specified number of days
    /// Example: NewerThan = 2 means emails from last 2 days
    /// </summary>
    public int? NewerThan { get; set; }

    public string BuildQueryString()
    {
        var queryParts = new List<string>();
        
        // Sender filter
        if (!string.IsNullOrWhiteSpace(SenderEmail))
        {
            queryParts.Add($"from:{SenderEmail}");
        }
        
        // Unread filter
        if (UnreadOnly)
        {
            queryParts.Add("in:unread");
        }
        
        // Date filters - Gmail format is yyyy/MM/dd
        if (On.HasValue)
        {
            // Search for emails on a specific date
            queryParts.Add($"after:{On.Value:yyyy/MM/dd}");
            queryParts.Add($"before:{On.Value.AddDays(1):yyyy/MM/dd}");
        }
        else
        {
            // After date filter
            if (After.HasValue)
            {
                queryParts.Add($"after:{After.Value:yyyy/MM/dd}");
            }
            
            // Before date filter
            if (Before.HasValue)
            {
                queryParts.Add($"before:{Before.Value:yyyy/MM/dd}");
            }
        }
        
        // Relative date filters (take precedence if set)
        if (OlderThan.HasValue)
        {
            queryParts.Add($"older_than:{OlderThan.Value}d");
        }
        
        if (NewerThan.HasValue)
        {
            queryParts.Add($"newer_than:{NewerThan.Value}d");
        }
        
        return string.Join(" ", queryParts);
    }
}
