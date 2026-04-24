# Repo Index — GymClassBooking

> Machine-readable codebase map. AI agents: read this before exploring with file-system tools.
> Updated manually when structure changes. Last updated: 2026-04-25.

---

## Solution

```yaml
solution_file: GymClassBooking.slnx
format: .slnx (MSBuild Simplified, not .sln)
framework: net10.0
```

---

## Projects

```yaml
src:
  - GymClassBooking.Domain # Entities, enums — no dependencies
  - GymClassBooking.Application # Services, interfaces, DTOs, exceptions
  - GymClassBooking.Infrastructure # DbContext, DI registration, seeder
  - GymClassBooking.API # ASP.NET Core Web API host
  - GymClassBooking.Web # Blazor WebAssembly frontend

tests:
  - GymClassBooking.UnitTests # xUnit, 8 tests (BookingServiceTests.cs)
  - GymClassBooking.IntegrationTests # xUnit + WebApplicationFactory, 5 tests
```

---

## Layer → Key Files

### Domain (`src/GymClassBooking.Domain/`)

```
Entities/
  Booking.cs
  GymClass.cs
  Member.cs
Enums/
  BookingStatus.cs
  MembershipTier.cs
```

### Application (`src/GymClassBooking.Application/`)

```
Services/
  BookingService.cs          # IBookingService implementation
Interfaces/
  IAppDbContext.cs            # EF-agnostic DB contract
  IBookingService.cs          # booking operations contract
DTOs/
  BookingDtos.cs              # request/response/summary records
Exceptions/
  AppExceptions.cs            # ClassFullException, BookingNotFoundException, DuplicateBookingException
```

### Infrastructure (`src/GymClassBooking.Infrastructure/`)

```
DependencyInjection.cs        # AddInfrastructure() — sole DI entry point, no parameters
Persistence/
  AppDbContext.cs             # implements IAppDbContext
  DbSeeder.cs                 # DbSeeder.SeedAsync(db) — guarded with Any() check
```

### API (`src/GymClassBooking.API/`)

```
Program.cs                    # startup, DI, CORS, OpenAPI, seeder call
Controllers/
  BookingsController.cs       # /api/bookings
  ClassesController.cs        # /api/classes
  MembersController.cs        # /api/members
```

### Web — Blazor WASM (`src/GymClassBooking.Web/`)

```
Program.cs                    # registers HttpClient, GymApiClient, MudBlazor
Pages/
  Home.razor                  # /
  BookClass.razor             # /book/{classId:int}
  MemberBookings.razor        # /member/{memberId:int}/bookings
Services/
  GymApiClient.cs             # all HTTP calls — single API boundary
Models/
  ApiModels.cs                # Web-side mirror of Application DTOs
Layout/
  App.razor                   # Blazor app root
```

### Integration Tests (`tests/GymClassBooking.IntegrationTests/`)

```
BookingsApiTests.cs           # 5 tests, IClassFixture<WebApplicationFactory<Program>> + IAsyncLifetime
```

### Unit Tests (`tests/GymClassBooking.UnitTests/`)

```
BookingServiceTests.cs        # 8 tests, Guid-named InMemory DB per test
```

---

## DI Registration Chain

```
Program.cs (API)
└── builder.Services.AddInfrastructure()
    ├── AddDbContext<AppDbContext>(UseInMemoryDatabase("GymClassBooking"))
    ├── AddScoped<IAppDbContext>(→ AppDbContext)
    └── AddScoped<IBookingService, BookingService>()

Program.cs (Web/Blazor)
├── AddScoped<HttpClient>(BaseAddress = https://localhost:7000/)
├── AddScoped<GymApiClient>()
└── AddMudServices()
```

---

## API Routes

| Controller         | Base Route      | Notable Endpoints                      |
| ------------------ | --------------- | -------------------------------------- |
| ClassesController  | `/api/classes`  | GET (upcoming), GET /{id}              |
| BookingsController | `/api/bookings` | POST, DELETE /{id}, GET /waitlist/{id} |
| MembersController  | `/api/members`  | GET, GET /{id}                         |

---

## CORS

```yaml
policy_name: BlazorClient
allowed_origins:
  - https://localhost:7185 # Blazor WASM https (primary)
  - http://localhost:5042 # Blazor WASM http
api_https_port: 7157
api_http_port: 5245
blazor_http_client_base_address: http://localhost:5245/
configured_in: src/GymClassBooking.API/Program.cs
```

---

## Database

```yaml
provider: EF Core InMemory
database_name: GymClassBooking
migrations: none
seeder: DbSeeder.SeedAsync(AppDbContext db)
seeder_guard: if (db.Members.Any()) return;
seeder_called_from: Program.cs on startup + IntegrationTests.InitializeAsync
```

---

## Test Patterns

```yaml
unit_tests:
  pattern: fresh Guid-named InMemory DB per test
  seed: only what the test needs (no DbSeeder)
  file: BookingServiceTests.cs

integration_tests:
  pattern: IClassFixture<WebApplicationFactory<Program>> + IAsyncLifetime
  InitializeAsync: EnsureDeletedAsync → EnsureCreatedAsync → DbSeeder.SeedAsync
  no_provider_override: true # uses same InMemory provider as app
  file: BookingsApiTests.cs
```
