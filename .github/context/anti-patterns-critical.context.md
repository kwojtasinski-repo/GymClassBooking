# Anti-Patterns — Critical (Block Merge)

> Violations of these rules are **not style nits** — they break the architecture, tests, or production behaviour.
> AI agents must check this list before generating code. Reviewers must reject PRs that contain any of these patterns.

---

## 1. No Business Logic in Controllers

**Rule:** Controllers contain exactly three steps: validate input → call service → map exception to HTTP status.

**Forbidden:** LINQ, EF queries, calculations, business conditions, `if (capacity > ...)` logic inside action methods.

```csharp
// ❌ FORBIDDEN
[HttpPost]
public async Task<IActionResult> Book(CreateBookingRequest req)
{
    var count = await _db.Bookings.CountAsync(b => b.ClassId == req.ClassId && b.Status == BookingStatus.Confirmed);
    if (count >= gymClass.Capacity) ...
}

// ✅ CORRECT
[HttpPost]
public async Task<IActionResult> Book(CreateBookingRequest req)
{
    try { var result = await _bookingService.BookClassAsync(req); return Created(..., result); }
    catch (ClassFullException) { return Conflict(); }
}
```

---

## 2. No EF Core Access Outside Infrastructure and Application

**Rule:** Only `AppDbContext` (Infrastructure) and services in the Application layer may touch EF entities or run LINQ queries.

**Forbidden:** Injecting `AppDbContext` into controllers, Blazor pages, or Domain classes.

---

## 3. No SQLite / No Migrations

**Rule:** `AddInfrastructure()` uses `UseInMemoryDatabase("GymClassBooking")`. No Sqlite packages, no `Migrations/` folder, no `Add-Migration`, no connection strings.

**Why:** Dual-provider conflicts destroy `WebApplicationFactory`-based integration tests (AD-003).

---

## 4. Never Use Swashbuckle

**Rule:** OpenAPI is provided by the built-in `Microsoft.AspNetCore.OpenApi` package (`AddOpenApi()` / `MapOpenApi()`).

**Forbidden:** `Swashbuckle.AspNetCore`, `AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`.

**Why:** Swashbuckle 10 / Microsoft.OpenApi 2.x causes `ReflectionTypeLoadException` in integration tests (AD-004).

---

## 5. No `.Result` or `.Wait()`

**Rule:** Every async call must use `await`. No `.Result`, `.GetAwaiter().GetResult()`, or `.Wait()` anywhere.

**Why:** Deadlocks in ASP.NET Core synchronization context.

---

## 6. DTOs Must Be Records

**Rule:** All DTOs (request, response, summary) must use `record`, not `class`.

```csharp
// ✅ CORRECT
public record CreateBookingRequest(int ClassId, int MemberId);

// ❌ FORBIDDEN
public class CreateBookingRequest { public int ClassId { get; set; } }
```

---

## 7. Blazor Pages Must Use GymApiClient

**Rule:** Blazor pages communicate with the backend exclusively via `GymApiClient`. No direct `HttpClient`, no EF access.

---

## 8. Controller Route Convention

**Rule:** Routes follow `/api/{resource}` (plural, lowercase). Use `[ApiController]` + `[Route("api/[controller]")]`. Do not duplicate route segments in `[HttpGet]`, `[HttpPost]` etc.

---

## 9. AddInfrastructure() Takes No Parameters

**Rule:** The `AddInfrastructure()` extension method signature is `IServiceCollection AddInfrastructure(this IServiceCollection)`. No database options parameter, no environment string, no override hook.

---

## Maintenance

When a new critical violation is discovered during review or caught in `agent-decisions.md` (after 2+ occurrences), add a new numbered entry above this line.
