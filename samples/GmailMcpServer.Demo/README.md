# GmailMcpServer.Demo

A minimal ASP.NET 10 web application that demonstrates how to mount the **Email.Attachments.McpServer** package as a running MCP server.

Run the app and point any MCP-compatible client (Claude Desktop, VS Code GitHub Copilot, …) at `http://localhost:5000/mcp` to call the Gmail tools directly from your AI assistant.

---

## Prerequisites

| Requirement | Notes |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | `dotnet --version` should show `10.x` |
| Google Cloud project | Enable the **Gmail API** and create an **OAuth 2.0 Desktop client** |

---

## Quick Start

### 1 — Get Google credentials

1. Open the [Google Cloud Console](https://console.cloud.google.com/).
2. Create or select a project and enable the **Gmail API**.
3. Go to **APIs & Services → Credentials → Create credentials → OAuth client ID → Desktop app**.
4. Download the JSON file and save it as `credentials.json` in the project root (next to `Program.cs`).

```
samples/GmailMcpServer.Demo/
  credentials.json          ← paste your downloaded file here
  credentials.json.example  ← the expected format (committed as reference)
```

> **Note:** `credentials.json` is listed in `.gitignore` — it will never be committed.

### 2 — Configure vendor mappings

Edit `appsettings.json` to map vendor email addresses to local folder names:

```json
"emailsAttachmentsDestination": {
  "Values": {
    "invoices@myvendor.com": "MyVendor",
    "billing@another.com":   "Another"
  }
}
```

Attachments are saved to `./invoices/<FolderName>/` by default. Change `storage:fileSystem:baseDirectory` to relocate them.

### 3 — Run

```bash
cd samples/GmailMcpServer.Demo
dotnet run
```

On first run, a browser window will open asking you to authorise the app with your Google account. The token is cached in `token.json` for subsequent runs.

The MCP endpoint is now available at:

```
http://localhost:5000/mcp
```

---

## Connecting an AI Client

### VS Code — GitHub Copilot

A `.vscode/mcp.json` is already included in this project. Open the folder in VS Code and GitHub Copilot will discover the server automatically.

### Claude Desktop

Add the following to `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "gmail-invoices": {
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

---

## Available MCP Tools

| Tool | Description |
|---|---|
| `list_vendors` | List configured vendor email-to-folder mappings |
| `search_emails` | Search Gmail inbox by sender / date range (metadata only) |
| `fetch_attachments_last_week` | Download last calendar week's attachments |
| `fetch_attachments_last_month` | Download last calendar month's attachments |
| `fetch_attachments_by_period` | Download a custom date range of attachments |
| `fetch_attachments_by_vendor` | Download attachments from a specific sender |
| `get_storage_stats` | Statistics on already-persisted files |

---

## Switching to UserDelegated Mode

The demo runs in **ServiceAccount** mode by default (one shared Gmail identity). To let each HTTP caller authenticate with their own Google account, see the commented-out block in `Program.cs` labelled **Option B: UserDelegated mode**.

---

## Project Structure

```
samples/GmailMcpServer.Demo/
  Program.cs                      — app entry point
  appsettings.json                — vendor mappings + storage config
  appsettings.Development.json    — dev-environment log overrides
  credentials.json.example        — OAuth2 credentials template
  .vscode/mcp.json                — VS Code / GitHub Copilot MCP config
  GmailMcpServer.Demo.csproj      — web project referencing Email.Attachments.McpServer
```
