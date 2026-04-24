---
name: create-unit-test
description: "Use when adding or modifying unit tests for services or domain logic. Covers Guid-named InMemory DB pattern, test naming conventions, arrange-act-assert structure, and seeding patterns for this project."
---

# Skill: Create Unit Test

Use this skill when adding tests to `tests/GymClassBooking.UnitTests/`.

## Rules

- Each test class creates its own `AppDbContext` using a **Guid-named InMemory database** — guarantees full isolation.
- No mocking of `AppDbContext` or `IAppDbContext` — use the real InMemory context.
- Seed only the data needed for the specific test — do not call `DbSeeder.SeedAsync` in unit tests.
- Test one behaviour per `[Fact]` — name pattern: `MethodName_Scenario_ExpectedResult`.

## File location

```
tests/GymClassBooking.UnitTests/{ServiceName}Tests.cs
```

## Template

```csharp
using GymClassBooking.Application.DTOs;
using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Services;
using GymClassBooking.Domain.Entities;
using GymClassBooking.Domain.Enums;
using GymClassBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.UnitTests;

public class {ServiceName}Tests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task {MethodName}_{Scenario}_{ExpectedResult}()
    {
        // Arrange
        await using var db = CreateDb();
        var service = new {ServiceName}(db);

        // Seed minimal data
        var member = new Member { FullName = "Test User", Email = "test@example.com", MembershipTier = MembershipTier.Standard };
        db.Members.Add(member);
        await db.SaveChangesAsync();

        // Act
        var result = await service.{MethodName}(/* args */);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result.Property);
    }

    [Fact]
    public async Task {MethodName}_{ErrorScenario}_Throws{ExceptionType}()
    {
        // Arrange
        await using var db = CreateDb();
        var service = new {ServiceName}(db);

        // Act & Assert
        await Assert.ThrowsAsync<{ExceptionType}>(() =>
            service.{MethodName}(/* args that trigger exception */));
    }
}
```

## Naming conventions

| Scenario type | Name pattern                       | Example                                              |
| ------------- | ---------------------------------- | ---------------------------------------------------- |
| Happy path    | `Method_Condition_ReturnsExpected` | `BookClass_ValidRequest_ReturnsConfirmed`            |
| Exception     | `Method_Condition_Throws{Type}`    | `BookClass_DuplicateBooking_ThrowsConflictException` |
| State change  | `Method_Condition_SetsProperty`    | `CancelBooking_LateCancel_SetsLateCancelFlag`        |

## Common assertions

```csharp
// Value equality
Assert.Equal("Confirmed", result.Status);

// Non-null
Assert.NotNull(result);

// Exception thrown
await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(-1));

// Collection
Assert.Single(results);
Assert.Empty(results);
Assert.Equal(3, results.Count);

// DB state after mutation
var entity = await db.Members.FindAsync(member.Id);
Assert.True(entity!.LateCancel);
```

## Do NOT

- Use `Moq` to mock `AppDbContext` — use InMemory instead.
- Call `DbSeeder.SeedAsync` in unit tests — seed only what the test needs.
- Share DbContext instances between tests — each `[Fact]` gets its own via `CreateDb()`.
- Assert on UI or HTTP concerns — unit tests target Application services only.
