# Snake game

Multiplayer snake game that utilizes SignalR for real-time communication and JWT for player authentication.

## Prerequisites

- .NET 7.0+
- Postgresql server
- node v18.13.0+
- pnpm/npm/yarn

## Setup

- If using other then pnpm, change pnpm commands located in snake-game.csproj to chosen package manager's
- Install `$ dotnet tool install --global dotnet-ef`
- Run `$ dotnet ef database update`

## Quick start

- Start Postgresql server
- `$ dotnet run` (or run through visual studio)

## Troubleshooting

- If there's an issue when trying to register or login, try creating new user in Postgres and editing connection string located in appsettings.json accordingly.

## Controls

- `w`, `a`, `s`, `d` or `arrow keys` - changes snake's direction

## Screenshots

Login page
<p align=center>
  <img src="./screenshots/login.png">
</p>

Game mode selection page
<p align=center>
  <img src="./screenshots/game_mode_selection.png">
</p>

In-game screen
<p align=center>
  <img src="./screenshots/ingame.png">
</p>
