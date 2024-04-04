# Snake game

This is a real-time, 2D multiplayer snake game that runs in a web browser. It's built using ASP.NET Core, React.js frameworks and SignalR for real-time communication between the server and the clients. Users are authenticated and authorized using JWT. The game logic and data processing is implemented in the backend, while the frontend is responsible for receiving user input and rendering the game itself.

## Prerequisites

- .NET 7.0+
- PostgreSQL database server
- Node v18.13.0+
- Node package manager: pnpm/npm/yarn

## Setup

- If you are using a package manager other than `pnpm`, update the `pnpm` commands in the `snake-game.csproj` file to match your chosen node package manager's commands.
- Install dotnet-ef: `$ dotnet tool install --global dotnet-ef`
- Update database to the last migration: `$ dotnet ef database update`

## Quick start

1. Start PostgreSQL server
2. Start backend:
   1. `$ cd backend`
   2. `$ dotnet run`
3. Start frontend:
   1. `$ cd frontend`
   2. `$ pnpm run dev`

## Troubleshooting

- If you encounter any issues while attempting to register or log in, consider:
  1. creating a new user in PostgreSQL db server
  2. adjusting the connection string in the `appsettings.json` file accordingly

## Controls

- `w`, `a`, `s`, `d` or `arrow` keys - to change snake's movement direction

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

Map creation page

<p align=center>
  <img src="./screenshots/map_creator.png">
</p>
