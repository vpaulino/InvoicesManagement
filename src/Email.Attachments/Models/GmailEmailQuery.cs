namespace ExtractLoadInvoices.Models;

/// <summary>
/// Gmail-specific email query with Gmail search operators.
/// Public for users to create Gmail queries.
/// </summary>
public sealed class GmailEmailQuery : EmailQuery
{
    public bool IncludeSpamTrash { get; set; } = false;
    public string LabelId { get; set; } = "INBOX";

    /// <summary>
    /// Builds Gmail-specific search query string (internal use)
    /// </summary>
    internal string BuildGmailQueryString()
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
