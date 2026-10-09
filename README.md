# Gin Rummy 2D

Two-player Gin Rummy for a local network, built for the course *Tecnologías para la
Construcción de Software* (Licenciatura en Ingeniería de Software, Facultad de Estadística e
Informática, Universidad Veracruzana).

Team 6: Nicolás Yazid Cruz Hernández and Isaac Adriano Vázquez Torres.

The system follows the client-server model required by the course: a server process owns
the state of every match and is the only component meant to reach the database, and a WPF
client runs on the machine of each player. Both communicate over TCP inside a LAN; HTTP and
WebSockets are not used.

## Current state

| Area | State |
| :--- | :--- |
| Client screens | The 25 screens of the prototype (P01 to P25) exist and are fully internationalized in `es-MX` and `en-US`. |
| Real data | Sign-up, sign-in and the global leaderboard read and write the database. Every other screen still runs on `Services/SampleDataService.cs`. |
| Server | Console host that publishes the leaderboard service with WCF over `net.tcp`. The client does not call it yet. |
| Database access | The client still reaches the database directly through Entity Framework for the three flows above. This is temporary: the target is that only the server holds a connection string. |
| Game rules | Not implemented yet. The game table works on sample data. |
| Logging | The client writes its log to `%LOCALAPPDATA%\GinRummy\client.log` through `ILogger<T>`. The server does not log yet. |
| Automated tests | None yet. The course requires at least 300. |

## Solution layout

| Project | Path | Responsibility |
| :--- | :--- | :--- |
| `GinRummy.Domain` | `GinRummy.Domain/` | Entities, DAO and security ports, rank resolution. Depends on no other project. |
| `GinRummy.Application` | `GinRummy.Application/` | Use cases. Today only `ViewLeaderboardUseCase`. |
| `GinRummy.Contracts` | `GinRummy.Contracts/` | WCF service contracts and their data contracts, shared by client and server. |
| `GinRummy.Data.EntityFramework` | `GinRummy.Data.EntityFramework/` | Entity Framework 6 adapter: context, DAOs and unit of work. |
| `GinRummy.Security` | `GinRummy.Security/` | Argon2id password hashing, verification code generation and SHA-256 code hashing. |
| `GinRummy.Server` | `GinRummy.Server/` | Composition root of the server and WCF host. |
| `GinRummy.Client` | `src/GinRummy.Client/` | WPF client: windows, modals, controllers, localization, styles and drawing controls. |

The code is organized as ports and adapters: the domain declares the interfaces it needs
(`IPlayerDao`, `IPasswordHasher`, ...) and each technology implements them in its own
project. The only places that know the concrete classes are the composition roots,
`GinRummy.Server/Program.cs` and `src/GinRummy.Client/App.xaml.cs`.

## Requirements

- Windows with Visual Studio 2022 or later, workload **.NET desktop development** and the
  **.NET Framework 4.8 targeting pack**. The projects target .NET Framework 4.8 with C# 7.3.
- SQL Server 2025 Express or later. The schema uses the native `JSON` type, which earlier
  versions do not have.
- SQL Server in mixed authentication mode. The application never connects with Windows
  authentication or with an administrative account.

NuGet packages (Entity Framework 6.5.2, Konscious.Security.Cryptography.Argon2 1.3.1 and
Microsoft.Extensions.Logging.Abstractions 3.1.32) are restored on build; the `packages/` folder
is not versioned.

## Database

The schema is created by SQL scripts, never by Entity Framework: the context disables its
initializer. Run them with an administrative account, in this order:

1. `crear_base_datos.sql`: creates `GinRummy_Dev` with collation `Latin1_General_100_CI_AS_SC_UTF8`.
2. `crear_tablas.sql`: creates the 30 tables with their keys and constraints.
3. The test data script.
4. `permisos_app.sql`: creates the login `GinRummyApp` and grants it `SELECT`, `INSERT` and
   `UPDATE` on schema `dbo`, nothing else. Type the password in your local copy only.

The scripts are currently kept with the design documents, outside this repository.

## Local configuration

Credentials are never committed. Each project that opens a connection reads it from a
`ConnectionStrings.config` file that git ignores:

| Project | File to create | Connection name |
| :--- | :--- | :--- |
| Client | `src/GinRummy.Client/ConnectionStrings.config` | `GinRummyContext` |
| Server | `GinRummy.Server/ConnectionStrings.config` | `GinRummyDb` |

1. Copy `ConnectionStrings.template.config` as `ConnectionStrings.config` in the same folder.
2. Set `Data Source` to your instance (for example `localhost\SQLEXPRESS`) and `User ID` and
   `Password` to the `GinRummyApp` account.
3. Build. The server project does not build while its file is missing.

Before every commit, check that `git status` does not list either `ConnectionStrings.config`.

## Running

1. Start `GinRummy.Server`. It listens on `net.tcp://localhost:8000/RankingsService` and
   stops when Enter is pressed in its console.
2. Start `GinRummy.Client`. It opens on the main menu (P01).

Both can be started together from Visual Studio with *Configure Startup Projects*.

## Internationalization

No visible text is written in XAML or in C#. Every string is identified by a key, following
the convention `Screen_TypeName` of the internationalization dictionary, and is resolved
against the resource file of the active culture.

- `Resources/Strings.resx` holds the `es-MX` strings and is the neutral resource
  (`NeutralResourcesLanguage("es-MX")`). `Resources/Strings.en-US.resx` compiles into a
  satellite assembly.
- `Localization/LocalizationProvider` exposes an indexer that XAML binds to:
  `{Binding [Shared_BtnLogIn], Source={StaticResource Loc}}`. Changing the culture raises
  `Item[]`, so every open window refreshes without being reopened.
- The controllers return message keys, never text. Messages with variable values use format
  strings with placeholders, filled with the active culture; sentences are never concatenated.
- Text that comes from the database and is shown to the player (ban reasons, report
  categories, ranks) is meant to be read from its `*Locale` translation table.

Adding a culture today means adding a `Strings.xx-XX.resx` file with the same keys and an
entry in `LocalizationProvider.AvailableCultures`, then rebuilding the client.

## Visual layer

- `Styles/Theme.xaml` centralizes colours, typefaces and control styles. A screen changes its
  look through these tokens, not through its own XAML or code.
- `Controls/PaintedBackground` draws the animated background of the menu and of some side
  panels. It is written from scratch, as required by CON-06, and computes each frame on the
  processor so that it renders the same with or without hardware acceleration.
- `Controls/PixelSurface`, `Controls/PixelBorder` and `Controls/PixelShapeCommon` draw
  the stepped pixel corners shared by buttons, panels, fields and cards.
- `Controls/CardBackCommon` paints the back of the cards once and every card reuses it.
- The typefaces and their terms are listed in `Fonts/LICENSES.md`. RetroByte declares no
  licence and HigherPixels is all rights reserved; both are pending review.

## Conventions

- Code follows the team's C# coding standard (version 8). `.editorconfig` applies the
  formatting and naming rules that Visual Studio can check.
- Every `catch` block that handles a failure without propagating it records it through the
  `ILogger<T>` that the class receives in its constructor.
- Each window documents in its summary the prototype screen (Pxx) and the use cases (CU-xx) it
  implements.
- Commit messages follow the form `type(scope): description` in English, for example
  `feat(client): open the lobby after a successful sign-in`. Build output, restored packages
  and local configuration are never committed.

## Pending work

These parts of the implemented use cases depend on the server and are not done yet:

- CU-02: the session, the ban and suspension checks and the two-step verification after a
  successful sign-in.
- CU-01: delivering the verification code by email and checking the typed code against its
  stored hash. Until then the code is shown on screen.
- CU-19: the friends leaderboard, which still uses sample data, and the highlighted row of the
  signed-in player.
