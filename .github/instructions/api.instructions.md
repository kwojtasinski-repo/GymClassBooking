---
description: "Use when creating, editing, or modifying an ASP.NET Core Web API controller, endpoint, route, HTTP action method, or API response mapping. Covers controller structure, thin-controller rules, exception-to-HTTP status mapping, and route conventions for this project."
applyTo: "src/GymClassBooking.API/**"
---

# API Controller Rules

Read `.github/skills/create-api-controller/SKILL.md` before generating any controller code.

## Key rules (summary)

- Controllers are thin: validate input → call service → map exception → return result. No LINQ, no EF, no business logic.
- Route: `[ApiController]` + `[Route("api/[controller]")]`. Plural, lowercase resource names.
- Return types: `Ok()`, `Created()`, `NoContent()`, `NotFound()`, `Conflict()`, `UnprocessableEntity()`.
- Map exceptions from Application layer in the controller — never let them propagate to middleware.
- Use `AddOpenApi()` / `MapOpenApi()` — never Swashbuckle.
