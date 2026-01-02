using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using ExtractLoadInvoices.Models;
using GmailApi = Google.Apis.Gmail.v1;
using System.Globalization;

namespace ExtractLoadInvoices.Services;

/// <summary>
/// Gmail implementation of IEmailService with mapping to provider-agnostic models
/// </summary>
public class GmailService : IEmailService
{
    private readonly GmailApi.GmailService _gmailApiService;
    private const string UserId = "me";

    public GmailService(GmailApi.GmailService gmailApiService)
    {
        _gmailApiService = gmailApiService;
    }

    public virtual async Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailQuery query)
    {
        var gmailQuery = query as GmailEmailQuery 
            ?? throw new ArgumentException("Query must be of type GmailEmailQuery for Gmail provider", nameof(query));

        var listRequest = _gmailApiService.Users.Messages.List(UserId);
        listRequest.LabelIds = gmailQuery.LabelId;
        listRequest.IncludeSpamTrash = gmailQuery.IncludeSpamTrash;
        listRequest.Q = gmailQuery.BuildGmailQueryString();

        var response = await listRequest.ExecuteAsync();

        return response?.Messages?.Select(MapToEmailMessage) ?? Enumerable.Empty<EmailMessage>();
    }

    public virtual async Task<EmailMessageDetails> GetMessageDetailsAsync(string messageId)
    {
        var getRequest = _gmailApiService.Users.Messages.Get(UserId, messageId);
        var gmailMessage = await getRequest.ExecuteAsync();
        
        return MapToEmailMessageDetails(gmailMessage);
    }

    public virtual async Task<AttachmentData> GetAttachmentAsync(string messageId, string attachmentId)
    {
        var attachmentRequest = _gmailApiService.Users.Messages.Attachments.Get(UserId, messageId, attachmentId);
        var gmailAttachment = await attachmentRequest.ExecuteAsync();
        
        return MapToAttachmentData(gmailAttachment);
    }

    public virtual async Task MarkAsReadAsync(string messageId)
    {
        var modifyRequest = new ModifyMessageRequest
        {
            RemoveLabelIds = new[] { "UNREAD" }
        };

        await _gmailApiService.Users.Messages.Modify(modifyRequest, UserId, messageId).ExecuteAsync();
    }

    private static EmailMessage MapToEmailMessage(Message gmailMessage)
    {
        return new EmailMessage
        {
            Id = gmailMessage.Id ?? string.Empty,
            ThreadId = gmailMessage.ThreadId ?? string.Empty,
            LabelIds = gmailMessage.LabelIds ?? Enumerable.Empty<string>(),
            Snippet = gmailMessage.Snippet ?? string.Empty,
            InternalDate = gmailMessage.InternalDate ?? 0
        };
    }

    private static EmailMessageDetails MapToEmailMessageDetails(Message gmailMessage)
    {
        var details = new EmailMessageDetails
        {
            Id = gmailMessage.Id ?? string.Empty,
            ThreadId = gmailMessage.ThreadId ?? string.Empty,
            LabelIds = gmailMessage.LabelIds ?? Enumerable.Empty<string>(),
            Snippet = gmailMessage.Snippet ?? string.Empty,
            InternalDate = gmailMessage.InternalDate ?? 0,
            SizeEstimate = gmailMessage.SizeEstimate ?? 0,
            HistoryId = gmailMessage.HistoryId ?? 0,
            Raw = gmailMessage.Raw ?? string.Empty
        };

        if (gmailMessage.Payload != null)
        {
            details.Payload = MapToEmailMessagePart(gmailMessage.Payload);
            
            // Parse common headers
            var headers = gmailMessage.Payload.Headers ?? Enumerable.Empty<MessagePartHeader>();
            foreach (var header in headers)
            {
                switch (header.Name?.ToLowerInvariant())
                {
                    case "from":
                        details.From = ParseEmailAddress(header.Value);
                        break;
                    case "to":
                        details.To = ParseEmailAddresses(header.Value);
                        break;
                    case "subject":
                        details.Subject = header.Value ?? string.Empty;
                        break;
                    case "date":
                        if (DateTime.TryParse(header.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        {
                            details.Date = date;
                        }
                        break;
                }
            }
        }

        return details;
    }

    private static EmailMessagePart MapToEmailMessagePart(MessagePart gmailPart)
    {
        var part = new EmailMessagePart
        {
            PartId = gmailPart.PartId ?? string.Empty,
            MimeType = gmailPart.MimeType ?? string.Empty,
            Filename = gmailPart.Filename ?? string.Empty,
            Headers = gmailPart.Headers?.ToDictionary(h => h.Name ?? string.Empty, h => h.Value ?? string.Empty) 
                      ?? new Dictionary<string, string>(),
            Parts = gmailPart.Parts?.Select(MapToEmailMessagePart) ?? Enumerable.Empty<EmailMessagePart>()
        };

        if (gmailPart.Body != null)
        {
            part.Body = new EmailMessagePartBody
            {
                AttachmentId = gmailPart.Body.AttachmentId ?? string.Empty,
                Size = gmailPart.Body.Size ?? 0,
                Data = gmailPart.Body.Data ?? string.Empty
            };
        }

        return part;
    }

    private static AttachmentData MapToAttachmentData(MessagePartBody gmailAttachment)
    {
        return new AttachmentData
        {
            AttachmentId = gmailAttachment.AttachmentId ?? string.Empty,
            Size = gmailAttachment.Size ?? 0,
            Data = gmailAttachment.Data ?? string.Empty
        };
    }

    private static EmailAddress? ParseEmailAddress(string? emailString)
    {
        if (string.IsNullOrWhiteSpace(emailString))
            return null;

        // Simple parsing: "Name <email@domain.com>" or just "email@domain.com"
        var match = System.Text.RegularExpressions.Regex.Match(emailString, @"(.+?)\s*<(.+?)>|(.+)");
        if (match.Success)
        {
            if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                return new EmailAddress
                {
                    Name = match.Groups[1].Value.Trim(),
                    Address = match.Groups[2].Value.Trim()
                };
            }
            else
            {
                return new EmailAddress
                {
                    Name = string.Empty,
                    Address = match.Groups[3].Value.Trim()
                };
            }
        }

        return null;
    }

    private static IEnumerable<EmailAddress> ParseEmailAddresses(string? emailsString)
    {
        if (string.IsNullOrWhiteSpace(emailsString))
            return Enumerable.Empty<EmailAddress>();

        // Split by comma and parse each
        return emailsString.Split(',')
            .Select(e => ParseEmailAddress(e.Trim()))
            .Where(e => e != null)
            .Cast<EmailAddress>();
    }
}
