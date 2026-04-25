# GymClassBooking — Copilot Instructions

## Project overview

.NET 10 gym class booking system with waitlist. Blazor WASM frontend (MudBlazor), ASP.NET Core Web API, EF Core InMemory (no SQLite, no migrations).

## Technology stack

| Layer                | Technology                                                     |
| -------------------- | -------------------------------------------------------------- |
| Domain & Application | .NET 10 class libraries                                        |
| API                  | ASP.NET Core 10 Web API, built-in OpenAPI (`AddOpenApi`)       |
| Frontend             | Blazor WebAssembly 10, MudBlazor 9                             |
| Data                 | EF Core 10 InMemory — `UseInMemoryDatabase("GymClassBooking")` |
| Tests                | xUnit, `WebApplicationFactory<Program>`, EF Core InMemory      |

## Architecture rules

- **Domain** has zero dependencies on Infrastructure or API.
- **Application** owns all business logic — services, interfaces, DTOs, exceptions.
- **Controllers are thin**: validate input → call service → map exception to HTTP status → return result. No LINQ, no EF queries.
- **Infrastructure** owns DbContext and DI registration only.
- **Web (Blazor)** calls `GymApiClient` — never calls EF Core directly.

## .NET 10 conventions

- Use `record` for DTOs (immutable, value-based equality).
- Use primary constructors where appropriate.
- Prefer `async`/`await` throughout — no `.Result` or `.Wait()`.
- Nullable reference types are enabled in all projects (`<Nullable>enable</Nullable>`).
- Implicit usings are enabled — no need to add `using System;` etc. manually.
- Target framework: `net10.0` in every `.csproj`.

## API conventions

- Route prefix: `/api/{resource}` — plural, lowercase.
- Return types: `Ok(dto)`, `Created(location, dto)`, `NoContent()`, `NotFound()`, `Conflict()`, `UnprocessableEntity()`.
- Map exceptions from Application layer to HTTP status codes in the controller — **never** let exceptions propagate to middleware.
- Use `[ApiController]` + `[Route("api/[controller]")]` — do not duplicate route segments.
- OpenAPI: use `AddOpenApi()` + `MapOpenApi()` — **do not** use Swashbuckle (see AD-004).
- CORS policy name: `"BlazorClient"` — allows localhost:7185 (Blazor https) and localhost:5042 (Blazor http).

## Blazor WASM conventions

- All pages live in `src/GymClassBooking.Web/Pages/`.
- All API calls go through `GymApiClient` in `src/GymClassBooking.Web/Services/`.
- Use MudBlazor components (`MudCard`, `MudTable`, `MudChip`, `MudSnackbar`) — never raw HTML Bootstrap.
- Page directive: `@page "/route"` — use route parameters as `{Param:type}`.
- Inject `GymApiClient` and `ISnackbar` via `@inject`.
- Load data in `OnInitializedAsync()`.
- Models mirror API DTOs — live in `src/GymClassBooking.Web/Models/ApiModels.cs`.

## Database — EF Core InMemory

- `AddInfrastructure()` (no parameters) registers `UseInMemoryDatabase("GymClassBooking")`.
- No migrations, no connection strings, no SQLite.
- `DbSeeder.SeedAsync(db)` is called from `Program.cs` on startup.
- Seeder guard: `if (db.Members.Any()) return;` — safe to call multiple times.

## Testing conventions

- **Unit tests** (`GymClassBooking.UnitTests`): create a fresh `AppDbContext` per test using a Guid-named InMemory database. No mocking of DbContext.
- **Integration tests** (`GymClassBooking.IntegrationTests`): use `IClassFixture<WebApplicationFactory<Program>>` + `IAsyncLifetime`. In `InitializeAsync`: call `EnsureDeletedAsync` → `EnsureCreatedAsync` → `DbSeeder.SeedAsync` to guarantee clean state before each test.
- Never add a second EF Core provider to the DI container in tests — the InMemory provider registered by `AddInfrastructure` is used as-is.

## Context files — read before non-trivial work

> For the full routing table (task → which file to read), see `.github/instructions/docs-index.instructions.md`.

### anti-patterns-critical.context.md

- Location: `.github/context/anti-patterns-critical.context.md`
- **Violations here block merge.** Check before code review.
- Covers: no logic in controllers, no EF outside Application/Infrastructure, no SQLite, no Swashbuckle, no `.Result`/`.Wait()`, DTOs must be records, Blazor must use GymApiClient.

### repo-index.md

- Location: `.github/context/repo-index.md`
- Machine-readable codebase map: project list, key files per layer, DI chain, API routes, CORS config, test patterns.
- Read this instead of exploring the file system when you need structural facts.

## Skills — use these for common code generation tasks

Skills live in `.github/skills/`. Read the relevant `SKILL.md` before generating:

| Task                    | Skill                                                |
| ----------------------- | ---------------------------------------------------- |
| Add API controller      | `.github/skills/create-api-controller/SKILL.md`      |
| Add Blazor page         | `.github/skills/create-blazor-page/SKILL.md`         |
| Add Application service | `.github/skills/create-application-service/SKILL.md` |
| Add domain entity       | `.github/skills/create-domain-entity/SKILL.md`       |
| Add unit test           | `.github/skills/create-unit-test/SKILL.md`           |
| Add integration test    | `.github/skills/create-integration-test/SKILL.md`    |
| Change waitlist rule    | `.github/skills/create-waitlist-promotion/SKILL.md`  |
| Change cancel penalty   | `.github/skills/create-cancellation-policy/SKILL.md` |
| Add booking guard       | `.github/skills/create-booking-rule/SKILL.md`        |
| Add DTO                 | `.github/skills/create-dto/SKILL.md`                 |
