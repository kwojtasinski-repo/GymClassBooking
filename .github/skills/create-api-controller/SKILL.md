---
name: create-api-controller
description: "Use when adding, editing, or modifying an ASP.NET Core Web API controller, endpoint, route, or HTTP action method. Covers controller structure, route conventions, exception-to-HTTP mapping, and anti-patterns for this project."
---

# Skill: Create API Controller

Use this skill when adding a new ASP.NET Core Web API controller to `src/GymClassBooking.API/Controllers/`.

## Checklist before generating

1. Read `.github/context/agent-decisions.md` — check for relevant decisions.
2. Identify which Application service interface handles the feature.
3. Identify the HTTP verbs and routes needed.
4. Identify which exceptions from `AppExceptions.cs` map to which HTTP status codes.

## File location

```
src/GymClassBooking.API/Controllers/{Resource}Controller.cs
```

`{Resource}` is PascalCase singular noun (e.g., `Bookings`, `Classes`, `Members`).

## Template

```csharp
using GymClassBooking.Application.DTOs;
using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymClassBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class {Resource}Controller : ControllerBase
{
    private readonly I{Resource}Service _{camelResource}Service;

    public {Resource}Controller(I{Resource}Service {camelResource}Service)
    {
        _{camelResource}Service = {camelResource}Service;
    }

    // GET api/{resource}
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _{camelResource}Service.GetAllAsync();
        return Ok(result);
    }

    // GET api/{resource}/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _{camelResource}Service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // POST api/{resource}
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] {Resource}Request request)
    {
        try
        {
            var result = await _{camelResource}Service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ConflictException ex) { return Conflict(new { error = ex.Message }); }
        catch (BusinessRuleException ex) { return UnprocessableEntity(new { error = ex.Message }); }
    }

    // DELETE api/{resource}/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _{camelResource}Service.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
```

## Rules

- **No LINQ or EF Core in controllers.** All queries live in the Application service.
- **No business logic in controllers.** Controllers only: receive input → call service → map exception to HTTP → return result.
- Exception mapping:
  - `NotFoundException` → `NotFound(new { error = ... })`
  - `ConflictException` → `Conflict(new { error = ... })`
  - `BusinessRuleException` → `UnprocessableEntity(new { error = ... })`
- Route attribute on class: `[Route("api/[controller]")]` — do not repeat `api/` in method attributes.
- Use `[ApiController]` — automatic model validation, no need for `ModelState.IsValid`.
- Return `Created(location, dto)` for POST success, `NoContent()` for DELETE success.
- Inject only the service interface — never inject `AppDbContext` or `IAppDbContext` directly.

## Do NOT

- Add middleware or filters to controllers.
- Add Swashbuckle attributes — built-in OpenAPI infers from return types.
- Add `[Authorize]` unless the feature explicitly requires it.
