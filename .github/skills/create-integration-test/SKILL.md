---
name: create-integration-test
description: "Use when adding or modifying integration tests, WebApplicationFactory tests, or HTTP-level API tests. Covers IAsyncLifetime pattern, database reset, seeding, test isolation, and anti-patterns for this project."
---

# Skill: Create Integration Test

Use this skill when adding tests to `tests/GymClassBooking.IntegrationTests/`.

## Architecture

- Use `IClassFixture<WebApplicationFactory<Program>>` — one factory per test class.
- Implement `IAsyncLifetime` — reset and re-seed the InMemory DB before **each** test in `InitializeAsync`.
- The factory uses the same `AddInfrastructure()` registered InMemory provider — no overrides needed.
- The entire HTTP stack runs in-process — no network ports needed.

## File location

```
tests/GymClassBooking.IntegrationTests/{Resource}ApiTests.cs
```

## Template

```csharp
using GymClassBooking.Infrastructure.Persistence;
using GymClassBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace GymClassBooking.IntegrationTests;

public class {Resource}ApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;

    public {Resource}ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get{Resource}s_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/{resources}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<{ResponseDto}>>();
        Assert.NotNull(items);
        Assert.NotEmpty(items!);
    }

    [Fact]
    public async Task Post{Resource}_ValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/{resources}",
            new { /* request properties */ });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<{ResponseDto}>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Post{Resource}_InvalidRequest_Returns{ErrorCode}()
    {
        var response = await _client.PostAsJsonAsync("/api/{resources}",
            new { /* request that triggers conflict/422 */ });
        Assert.Equal(HttpStatusCode.{ErrorCode}, response.StatusCode);
    }

    [Fact]
    public async Task Delete{Resource}_ValidId_ReturnsNoContent()
    {
        // Get a known entity from seed data via DbContext
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = db.{Entities}.First();

        var response = await _client.DeleteAsync($"/api/{resources}/{entity.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
```

## Rules

- **`IAsyncLifetime.InitializeAsync` always**: `EnsureDeletedAsync` → `EnsureCreatedAsync` → `DbSeeder.SeedAsync`. This gives clean state for every test.
- Access seed data by querying the `AppDbContext` in a `CreateScope()` — never hard-code integer IDs.
- Each test uses `_client` (set in `InitializeAsync`) — do not create new clients per test.
- Test only HTTP contract: status code, response body shape. Do not assert on internal DB state.
- Route conventions: plural lowercase `/api/bookings`, `/api/classes`, `/api/members`.
- Keep tests independent — each test can run in any order because `InitializeAsync` resets state.

## Exception → HTTP status mapping (for assertions)

| Application exception   | Expected HTTP             |
| ----------------------- | ------------------------- |
| `NotFoundException`     | `404 NotFound`            |
| `ConflictException`     | `409 Conflict`            |
| `BusinessRuleException` | `422 UnprocessableEntity` |

## Do NOT

- Override `ConfigureWebHost` in a custom factory — the InMemory provider is already registered.
- Add a second EF Core provider — causes `InvalidOperationException: Services for database providers X, Y have been registered`.
- Hard-code entity IDs from seed data — IDs are assigned by InMemory EF Core and may change.
- Use `Thread.Sleep` or `Task.Delay` — all operations are synchronous in InMemory.
