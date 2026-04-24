---
name: create-application-service
description: "Use when adding or modifying a service, interface, or business logic in the Application layer. Covers service structure, IAppDbContext usage, exception throwing, and interface conventions for this project."
---

# Skill: Create Application Service

Use this skill when adding a new service to `src/GymClassBooking.Application/Services/`.

## Checklist before generating

1. Read `.github/context/agent-decisions.md` for decisions relevant to the feature area.
2. Read `docs/decisions.md` for design constraints (D-001, D-002, D-003...).
3. Identify which domain entities and DTOs the service operates on.
4. Identify the interface that controllers will depend on.

## Files to create/modify

| File                                                           | Action                                                |
| -------------------------------------------------------------- | ----------------------------------------------------- |
| `src/GymClassBooking.Application/Interfaces/I{Name}Service.cs` | Create interface                                      |
| `src/GymClassBooking.Application/Services/{Name}Service.cs`    | Create implementation                                 |
| `src/GymClassBooking.Infrastructure/DependencyInjection.cs`    | Register `AddScoped<I{Name}Service, {Name}Service>()` |

## Interface template

```csharp
namespace GymClassBooking.Application.Interfaces;

public interface I{Name}Service
{
    Task<{ResponseDto}> GetByIdAsync(int id);
    Task<IReadOnlyList<{ResponseDto}>> GetAllAsync();
    Task<{ResponseDto}> CreateAsync({RequestDto} request);
    Task DeleteAsync(int id);
}
```

## Implementation template

```csharp
using GymClassBooking.Application.DTOs;
using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Interfaces;
using GymClassBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.Application.Services;

public class {Name}Service : I{Name}Service
{
    private readonly IAppDbContext _db;

    public {Name}Service(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<{ResponseDto}> GetByIdAsync(int id)
    {
        var entity = await _db.{Entities}
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException($"{EntityName} {id} not found.");

        return Map(entity);
    }

    public async Task<IReadOnlyList<{ResponseDto}>> GetAllAsync()
    {
        var entities = await _db.{Entities}.ToListAsync();
        return entities.Select(Map).ToList();
    }

    public async Task<{ResponseDto}> CreateAsync({RequestDto} request)
    {
        // Validate business rules — throw ConflictException or BusinessRuleException as needed
        var entity = new {Entity}(/* map from request */);
        _db.Add(entity);
        await _db.SaveChangesAsync(CancellationToken.None);
        return Map(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.{Entities}
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException($"{EntityName} {id} not found.");
        _db.Remove(entity);
        await _db.SaveChangesAsync(CancellationToken.None);
    }

    private static {ResponseDto} Map({Entity} e) => new(/* map properties */);
}
```

## Rules

- **Inject only `IAppDbContext`** — no HttpClient, no other services unless explicitly required.
- **All LINQ/EF queries belong here** — never in controllers or domain entities.
- **Throw typed exceptions** from `AppExceptions.cs`:
  - `NotFoundException` — entity not found by ID.
  - `ConflictException` — uniqueness or duplicate violation.
  - `BusinessRuleException` — domain rule violated (e.g., late cancel flag).
- **Map domain → DTO inline or via a private static `Map` method** — do not expose domain entities from the service.
- Register in `DependencyInjection.cs` as `AddScoped<I{Name}Service, {Name}Service>()`.
- Keep methods `async Task` — do not use `.Result` or `.Wait()`.

## Do NOT

- Add HTTP-specific logic (status codes, routing) to services.
- Reference `Microsoft.AspNetCore.*` namespaces in Application layer.
- Add AutoMapper — map manually with `record` constructors.
