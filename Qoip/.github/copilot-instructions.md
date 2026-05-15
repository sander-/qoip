# Copilot Instructions

## Project Guidelines
- For this library, only use .NET and Microsoft packages; do not add third-party package dependencies.
- Use the repository's existing Request/Response patterns for new features; new probes and analyzers must follow these patterns and be JSON-serializable. If implementing a feature would require breaking those patterns, skip it for now.
- Use a NetworkSecurity folder/namespace for cross-cutting features spanning network and security concerns when logical.
- Prefer separate pages for Qoip-Web UI work; avoid combining multiple new features into a single page.