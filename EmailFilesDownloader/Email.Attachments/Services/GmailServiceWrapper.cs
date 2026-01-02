using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using ExtractLoadInvoices.Models;

namespace ExtractLoadInvoices.Services;

public class GmailServiceWrapper : IEmailService
{
    private readonly GmailService _gmailService;
    private const string UserId = "me";

    public GmailServiceWrapper(GmailService gmailService)
    {
        _gmailService = gmailService;
    }

    public async Task<IEnumerable<Message>> GetEmailsAsync(EmailQuery query)
    {
        var listRequest = _gmailService.Users.Messages.List(UserId);
        listRequest.LabelIds = query.LabelId;
        listRequest.IncludeSpamTrash = query.IncludeSpamTrash;
        listRequest.Q = query.BuildQueryString();

        var response = await listRequest.ExecuteAsync();

        return response?.Messages ?? Enumerable.Empty<Message>();
    }

    public async Task<Message> GetMessageDetailsAsync(string messageId)
    {
        var getRequest = _gmailService.Users.Messages.Get(UserId, messageId);
        return await getRequest.ExecuteAsync();
    }

    public async Task<MessagePartBody> GetAttachmentAsync(string messageId, string attachmentId)
    {
        var attachmentRequest = _gmailService.Users.Messages.Attachments.Get(UserId, messageId, attachmentId);
        return await attachmentRequest.ExecuteAsync();
    }

    public async Task MarkAsReadAsync(string messageId)
    {
        var modifyRequest = new ModifyMessageRequest
        {
            RemoveLabelIds = new[] { "UNREAD" }
        };

        await _gmailService.Users.Messages.Modify(modifyRequest, UserId, messageId).ExecuteAsync();
    }
}
