# ExtractLoadInvoices - Refactored Architecture

## ?? Project Structure

```
ExtractLoadInvoices/
??? Program.cs                          # Application entry point with DI setup
??? Configuration/
?   ??? AppSettings.cs                  # Root settings model
?   ??? GoogleCredentialsSettings.cs    # Google auth settings
?   ??? EmailMappingSettings.cs         # Email-to-folder mappings
?   ??? IConfigurationService.cs        # Interface
?   ??? ConfigurationService.cs         # Configuration loader/validator
??? Authentication/
?   ??? IGoogleAuthenticator.cs         # Interface
?   ??? GoogleAuthenticator.cs          # Google OAuth 2.0 implementation
??? Services/
?   ??? IGmailService.cs                # Interface
?   ??? GmailServiceWrapper.cs          # Gmail API wrapper
?   ??? IEmailProcessor.cs              # Interface
?   ??? EmailProcessor.cs               # Email processing orchestration
??? Attachments/
?   ??? IAttachmentFilter.cs            # Interface
?   ??? AttachmentFilter.cs             # MIME type filtering
?   ??? IAttachmentDownloader.cs        # Interface
?   ??? AttachmentDownloader.cs         # Attachment download & decode
??? FileSystem/
?   ??? IFileStorageService.cs          # Interface
?   ??? FileStorageService.cs           # File operations
??? Models/
?   ??? EmailQuery.cs                   # Email search criteria with date filtering
?   ??? EmailAttachment.cs              # Attachment data model
?   ??? ProcessingResult.cs             # Result tracking
??? Extensions/
    ??? Base64UrlExtensions.cs          # RFC 4648 base64url decoder
```

## ?? Key Responsibilities

### **Configuration Layer**
- Loads and validates application settings
- Manages Google API credentials
- Handles email-to-folder mappings

### **Authentication Layer**
- Performs Google OAuth 2.0 authentication
- Manages credential and token storage
- Handles automatic token refresh

### **Services Layer**
- **GmailServiceWrapper**: Encapsulates Gmail API operations
- **EmailProcessor**: Orchestrates the email processing workflow

### **Attachments Layer**
- **AttachmentFilter**: Validates and filters attachments by MIME type
- **AttachmentDownloader**: Downloads and decodes attachment data

### **FileSystem Layer**
- Manages directory creation
- Handles file storage operations
- Provides path management utilities

## ? **New Features**

### ?? **Date Filtering**
The `EmailQuery` model now supports comprehensive date-based filtering:

- **Absolute dates**: Filter by specific date ranges (`After`, `Before`, `On`)
- **Relative dates**: Filter by "last N days" or "older than N days" (`NewerThan`, `OlderThan`)
- **Gmail syntax**: Automatically generates proper Gmail search queries

**Quick Example:**
```csharp
var query = new EmailQuery
{
    SenderEmail = "invoices@company.com",
    NewerThan = 7,  // Last 7 days
    UnreadOnly = true
};

var emails = await gmailService.GetEmailsAsync(query);
```

**See:** [DATE_FILTERING_GUIDE.md](DATE_FILTERING_GUIDE.md) for complete documentation

## ??? Architecture Principles

### ? Separation of Concerns
Each class has a single, well-defined responsibility

### ? Dependency Injection
All dependencies are injected through constructors, enabling:
- Easy testing with mocks
- Loose coupling between components
- Better maintainability

### ? Interface-Based Design
All major components implement interfaces:
- Enables unit testing
- Allows for easy swapping of implementations
- Supports future extensions

### ? Logging Integration
Integrated Microsoft.Extensions.Logging:
- Console logging configured
- Structured logging throughout
- Different log levels for different scenarios

## ?? Usage

### **Using in Another Application**

The refactored code is now modular and reusable. You can reference individual services:

```csharp
// In your application
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<IGoogleAuthenticator, GoogleAuthenticator>();
services.AddSingleton<IEmailProcessor, EmailProcessor>();
// ... register other services as needed

// Use the services
var processor = serviceProvider.GetRequiredService<IEmailProcessor>();
var result = await processor.ProcessEmailsFromSenderAsync(
    "sender@example.com", 
    "invoices");
```

### **Running Standalone**

```bash
dotnet run
```

The application will:
1. Load configuration from `app.json`
2. Authenticate with Google OAuth 2.0
3. Process emails from each configured sender
4. Download PDF attachments to respective folders
5. Mark emails as read
6. Log processing results

## ?? Configuration

The `app.json` configuration format remains unchanged:

```json
{
  "applicationName": "InvoiceDownloader",
  "googleCredentials": {
    "Values": {
      "credentialsLocation": "credentials.json",
      "tokenDestination": "token.json"
    }
  },
  "emailsAttachmentsDestination": {
    "Values": {
      "sender@example.com": "destination-folder"
    }
  }
}
```

### Setting Up Google OAuth Credentials

**IMPORTANT**: The `Credentials.json` file contains sensitive OAuth credentials and should NEVER be committed to version control.

1. **Create OAuth Credentials in Google Cloud Console**:
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create or select a project
   - Enable the Gmail API
   - Create OAuth 2.0 credentials (Desktop application type)
   - Download the credentials JSON file

2. **Set Up Local Credentials**:
   - Copy `Credentials.json.example` to `Credentials.json`
   - Replace the placeholder values with your actual credentials from Google Cloud Console
   - The file is already in `.gitignore` and will not be committed

3. **Credentials File Format**:
   ```json
   {
     "installed": {
       "client_id": "YOUR_CLIENT_ID.apps.googleusercontent.com",
       "project_id": "YOUR_PROJECT_ID",
       "auth_uri": "https://accounts.google.com/o/oauth2/auth",
       "token_uri": "https://oauth2.googleapis.com/token",
       "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
       "client_secret": "YOUR_CLIENT_SECRET",
       "redirect_uris": ["http://localhost"]
     }
   }
   ```

## ?? Testing

Each component can now be unit tested independently:

```csharp
// Example: Testing EmailProcessor
var mockGmailService = new Mock<IGmailService>();
var mockAttachmentFilter = new Mock<IAttachmentFilter>();
var mockDownloader = new Mock<IAttachmentDownloader>();
var mockFileStorage = new Mock<IFileStorageService>();
var mockLogger = new Mock<ILogger<EmailProcessor>>();

var processor = new EmailProcessor(
    mockGmailService.Object,
    mockAttachmentFilter.Object,
    mockDownloader.Object,
    mockFileStorage.Object,
    mockLogger.Object);

// Test the processor...
```

## ?? Extension Points

### **Adding New Attachment Types**

Modify `AttachmentFilter.cs`:

```csharp
private readonly HashSet<string> _validMimeTypes = new()
{
    "application/pdf",
    "application/octet-stream",
    "image/jpeg", // Add new MIME type
};
```

### **Custom File Naming Strategy**

Create a new implementation of `IFileStorageService` with custom naming logic.

### **Multiple Email Accounts**

The architecture now supports processing multiple accounts by creating multiple `GmailService` instances.

## ?? Dependencies

- **Google.Apis.Gmail.v1**: Gmail API integration
- **Microsoft.Extensions.Hosting**: Dependency injection & hosting
- **Microsoft.Extensions.Logging**: Structured logging
- **Microsoft.Extensions.Configuration**: Configuration management

## ?? Benefits of Refactoring

1. **Testability**: All components can be unit tested
2. **Maintainability**: Clear separation of concerns
3. **Reusability**: Services can be used in other applications
4. **Extensibility**: Easy to add new features
5. **Observability**: Integrated logging for troubleshooting
6. **Type Safety**: Strongly-typed configuration models
7. **Error Handling**: Centralized error handling with detailed logging

## ?? Migration from Old Code

The old `Program.cs` monolithic approach has been refactored into:
- Configuration loading ? `ConfigurationService`
- Google authentication ? `GoogleAuthenticator`
- Gmail operations ? `GmailServiceWrapper`
- Email processing loop ? `EmailProcessor`
- Attachment filtering ? `AttachmentFilter`
- Attachment downloading ? `AttachmentDownloader`
- File operations ? `FileStorageService`

All functionality remains the same, but the code is now organized following OOP principles.
