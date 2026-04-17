# Changelog — Email.Attachments.McpServer

All notable changes to this package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [1.0.0] — 2026-04-17

### Added
- Initial release of `Email.Attachments.McpServer`.
- MCP Streamable-HTTP transport via `ModelContextProtocol.AspNetCore` 1.2.0.
- `AddGmailMcpServer()` extension on `IServiceCollection` — registers all
  services and the MCP server with one call.
- `MapGmailMcpServer()` extension on `IEndpointRouteBuilder` — mounts the MCP
  HTTP endpoint.
- **Authentication modes**
  - `GmailAuthMode.ServiceAccount` — reads `credentials.json` / `token.json`
    from disk; suitable for daemons and server-side apps.
  - `GmailAuthMode.UserDelegated` — reads the Google access-token from the
    current ASP.NET HTTP context; suitable for multi-user web apps.
- **MCP Tools**
  - `list_vendors` — list configured email-to-folder vendor mappings.
  - `search_emails` — search Gmail inbox by sender and date range (metadata only).
  - `fetch_attachments_last_week` — download last calendar week's attachments.
  - `fetch_attachments_last_month` — download last calendar month's attachments.
  - `fetch_attachments_by_period` — download attachments in a custom date range.
  - `fetch_attachments_by_vendor` — download attachments from a specific sender.
  - `get_storage_stats` — retrieve statistics on already-persisted files.
- Full compatibility with `Email.Attachments` 2.0.0 provider-agnostic API.
