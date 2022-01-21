// See https://aka.ms/new-console-template for more information
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


IConfiguration Configuration = new ConfigurationBuilder()
  .AddJsonFile(Path.Combine(Environment.CurrentDirectory, "app.json"))
  .AddEnvironmentVariables()
  .Build();


var emailsFilesDestinationMapper = new EmailsFilesDestinationMapper();
var googleCredentials = new GoogleCredentials();

Configuration.GetSection("emailsAttachmentsDestination").Bind(emailsFilesDestinationMapper);
Configuration.GetSection("googleCredentials").Bind(googleCredentials);

string googleApiCredentialsFileLocation = googleCredentials.Values.GetValueOrDefault("credentialsLocation");
string authenticatedTokenFileLocation = googleCredentials.Values.GetValueOrDefault("tokenDestination");
string ApplicationName = Configuration.GetValue<string>("applicationName");


string[] Scopes = { GmailService.Scope.GmailReadonly };

UserCredential credential;

using (var stream =
    new FileStream(googleApiCredentialsFileLocation, FileMode.Open, FileAccess.Read))
{
    // The file token.json stores the user's access and refresh tokens, and is created
    // automatically when the authorization flow completes for the first time.
    
    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        Scopes,
        "user",
        CancellationToken.None,
        new FileDataStore(authenticatedTokenFileLocation, true)).Result;
    Console.WriteLine("Credential file saved to: " + authenticatedTokenFileLocation);
}

// Create Gmail API service.
var gmailService = new GmailService(new BaseClientService.Initializer()
{
    HttpClientInitializer = credential,
    ApplicationName = ApplicationName,
});
 


foreach (var key in emailsFilesDestinationMapper.Values.Keys)
{

    var emailListRequest = gmailService.Users.Messages.List("me");
    emailListRequest.LabelIds = "INBOX";
    emailListRequest.IncludeSpamTrash = false;
    emailListRequest.Q = $"from:{key}";

    //emailListRequest.Q = "is:unread"; // This was added because I only wanted unread emails...

    // Get our emails
    var emailListResponse = await emailListRequest.ExecuteAsync();

    if (emailListResponse != null && emailListResponse.Messages != null)
    {
        // Loop through each email and get what fields you want...
        foreach (var message in emailListResponse.Messages)
        {
            Message messageFilled = gmailService.Users.Messages.Get("me", message.Id).Execute();

            if (messageFilled.Payload.Parts == null)
                continue;

            var parts = messageFilled.Payload.Parts.Where(part => !String.IsNullOrEmpty(part.Filename) && (part.MimeType.Equals("application/pdf") || part.MimeType.Equals("application/octet-stream")));
            
            foreach (MessagePart part in parts)
            {

                String attId = part.Body.AttachmentId;
                MessagePartBody attachPart = gmailService.Users.Messages.Attachments.Get("me", message.Id, attId).Execute();

                // Converting from RFC 4648 base64 to base64url encoding
                // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
                String attachData = attachPart.Data.Replace('-', '+');
                attachData = attachData.Replace('_', '/');

                byte[] data = Convert.FromBase64String(attachData);
                
                emailsFilesDestinationMapper.Values.TryGetValue(key, out var folderName);

                if (!Directory.Exists(Environment.CurrentDirectory + "/" + folderName)) 
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "/" + folderName);
                }
                File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory+"/"+folderName , part.Filename), data);
            }
        }
    }
}

public class EmailsFilesDestinationMapper
{
    public Dictionary<string, string> Values { get; set; }

}

public class GoogleCredentials
{

    public Dictionary<string, string> Values { get; set; }
}