using EmailAttachments.McpServer;
using EmailAttachments.McpServer.Options;

// ─────────────────────────────────────────────────────────────────────────────
// Gmail MCP Server — Demo Host
//
// This minimal ASP.NET 10 app shows how to expose Gmail attachment management
// as MCP tools using the Email.Attachments.McpServer package.
//
// Before running:
//   1. Copy credentials.json.example → credentials.json and fill in your
//      OAuth2 client credentials from the Google Cloud Console.
//   2. Set your vendor mappings in appsettings.json under
//      "emailsAttachmentsDestination:Values".
//   3. Run:  dotnet run
//   4. Point an MCP client at:  http://localhost:5000/mcp
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ── Option A: ServiceAccount mode (single fixed Gmail identity) ───────────────
//
// Uses credentials.json / token.json on disk.  All MCP tool calls run under
// the same Gmail account.  Ideal for daemons or single-user deployments.
//
builder.Services.AddGmailMcpServer(options =>
{
    options.AuthMode             = GmailAuthMode.ServiceAccount;
    options.ServerName           = "Gmail Invoices MCP Server";
    options.ServerVersion        = "1.0.0";
    options.CredentialsPath      = "credentials.json";  // from Google Cloud Console
    options.TokenPath            = "token.json";         // cached OAuth2 token (auto-created on first run)
    options.ApplicationName      = builder.Configuration.GetValue<string>("applicationName") ?? "GmailMcpDemo";
    options.BaseStorageDirectory = "./invoices";
}, builder.Configuration); // passes vendor mappings + storage settings from appsettings.json

// ── Option B: UserDelegated mode (per-user Google OAuth) ─────────────────────
//
// Each HTTP caller authenticates with their own Google account.
// Requires SaveTokens = true in the Google handler.
// Uncomment the block below and comment out Option A above to use this mode.
//
// using Microsoft.AspNetCore.Authentication.Cookies;
//
// builder.Services
//     .AddAuthentication(o =>
//     {
//         o.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
//         o.DefaultChallengeScheme = "Google";
//     })
//     .AddCookie()
//     .AddGoogle(o =>
//     {
//         o.ClientId     = builder.Configuration["Google:ClientId"]!;
//         o.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
//         o.SaveTokens   = true;   // ← required: persists access_token to the auth cookie
//         o.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");
//         o.Scope.Add("https://www.googleapis.com/auth/gmail.modify");
//     });
//
// builder.Services.AddGmailMcpServer(options =>
// {
//     options.AuthMode      = GmailAuthMode.UserDelegated;
//     options.ServerName    = "Gmail Invoices MCP Server";
//     options.ServerVersion = "1.0.0";
// });

var app = builder.Build();

// ── Uncomment these two lines when using Option B (UserDelegated) ─────────────
// app.UseAuthentication();
// app.UseAuthorization();

// ── Mount the MCP endpoint ────────────────────────────────────────────────────
//
// Exposes a Streamable-HTTP MCP endpoint at /mcp.
// Compatible with Claude Desktop, VS Code GitHub Copilot, and any MCP client.
//
// To restrict access to authenticated users (UserDelegated mode):
//   app.MapGmailMcpServer("/mcp").RequireAuthorization();
//
app.MapGmailMcpServer("/mcp");

app.Run();
