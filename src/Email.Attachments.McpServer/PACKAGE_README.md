# Email.Attachments.McpServer

Expose Gmail attachment management as **MCP (Model Context Protocol) tools** inside any ASP.NET 10 web application using the standard `AddXxx` / `MapXxx` pattern.

AI assistants that support MCP (Claude, GitHub Copilot, etc.) can then call the tools directly to list, search, and download email attachments from a configured Gmail inbox.

---

## Quick Start

### 1 — Install the package

```bash
dotnet add package Email.Attachments.McpServer
```

### 2 — Configure in `Program.cs`

#### Service-account mode (single shared Gmail identity)

```csharp
using EmailAttachments.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGmailMcpServer(options =>
{
    options.AuthMode       = GmailAuthMode.ServiceAccount;
    options.ServerName     = "My Gmail MCP Server";
    options.ServerVersion  = "1.0.0";
    options.CredentialsPath = "credentials.json";   // from Google Cloud Console
    options.TokenPath       = "token.json";          // cached OAuth token
    options.ApplicationName = "MyApp";
    options.BaseStorageDirectory = "./invoices";
}, builder.Configuration);   // optional — binds vendor mappings from appsettings.json

var app = builder.Build();

app.MapGmailMcpServer("/mcp");          // MCP endpoint at /mcp
// app.MapGmailMcpServer("/mcp").RequireAuthorization();  // add auth gate

app.Run();
```

#### User-delegated mode (each user connects with their own Google account)

```csharp
using EmailAttachments.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId     = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        options.SaveTokens   = true;   // ← required
        options.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");
        options.Scope.Add("https://www.googleapis.com/auth/gmail.modify");
    });

builder.Services.AddGmailMcpServer(options =>
{
    options.AuthMode      = GmailAuthMode.UserDelegated;
    options.ServerName    = "My Gmail MCP Server";
    options.ServerVersion = "1.0.0";
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Require the caller to be authenticated before reaching the MCP endpoint
app.MapGmailMcpServer("/mcp").RequireAuthorization();

app.Run();
```

---

## Available MCP Tools

| Tool name | Description |
|---|---|
| `list_vendors` | List all configured vendor email-to-folder mappings |
| `search_emails` | Search Gmail inbox by sender, date range (metadata only) |
| `fetch_attachments_last_week` | Download attachments from the last calendar week |
| `fetch_attachments_last_month` | Download attachments from the last calendar month |
| `fetch_attachments_by_period` | Download attachments within a custom date range |
| `fetch_attachments_by_vendor` | Download attachments from a specific sender |
| `get_storage_stats` | Get statistics on already-persisted files |

---

## Vendor Mappings (`appsettings.json`)

```json
{
  "applicationName": "MyApp",
  "emailsAttachmentsDestination": {
    "Values": {
      "invoices@vendor1.com": "vendor1",
      "billing@vendor2.com":  "vendor2"
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

## Connecting an AI Client

Point your MCP client at the server's `/mcp` endpoint.

**Example — Claude Desktop (`claude_desktop_config.json`):**

```json
{
  "mcpServers": {
    "gmail": {
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

**Example — VS Code GitHub Copilot (`.vscode/mcp.json`):**

```json
{
  "servers": {
    "gmail": {
      "type": "http",
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

---

## Related Packages

- [`Email.Attachments`](https://www.nuget.org/packages/Email.Attachments) — the underlying Gmail extraction library

## License

MIT — see [LICENSE](https://github.com/vpaulino/InvoicesManagement/blob/main/LICENSE).
