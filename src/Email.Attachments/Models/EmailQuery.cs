namespace ExtractLoadInvoices.Models;

/// <summary>
/// Base class for provider-agnostic email query parameters
/// </summary>
public abstract class EmailQuery
{
    public string SenderEmail { get; set; } = string.Empty;
    public bool UnreadOnly { get; set; } = true;
    
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
}
