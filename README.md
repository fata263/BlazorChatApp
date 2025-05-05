# Blazor Server Chat App (.NET 9)

A real-time chat application built with Blazor Server, SignalR, and EF Core (SQLite) — with emoji support, private/public messaging, and persistent history.

## Features

- ✅ Real-time SignalR messaging
- ✅ Private + public chat
- ✅ Emoji picker (emoji-button)
- ✅ SQLite persistence
- ✅ xUnit unit testing
- ✅ .NET 9 support
- ✅ Docker + GitHub Actions

## Project Structure

BlazorChatAppNet9/
├── BlazorChatApp.sln
├── BlazorChatApp/       # Main app
│   └── Pages/, Shared/, Data/, wwwroot/
├── BlazorChatApp.Tests/ # Unit tests

## How to Run

### Prerequisites
- .NET 9 SDK (preview)
- Visual Studio 2022 v17.13+ or CLI

### Run
- `dotnet run --project BlazorChatApp`

### Test
- `dotnet test BlazorChatApp.Tests`

## Docker

Build and run:
```bash
docker-compose up --build
```

## CI/CD

Uses GitHub Actions to:
- Checkout repo
- Restore, build, test
- Publish release

## Notes

- Chat history saved to `chat.db`
- Emojis inserted via JS
- Use prompt to choose username
