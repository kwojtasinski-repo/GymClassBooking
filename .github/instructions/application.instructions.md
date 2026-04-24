---
description: "Use when adding or modifying a service, interface, business logic, use case, or application-layer operation. Covers service structure, IAppDbContext usage, exception throwing conventions, and interface patterns for this project."
applyTo: "src/GymClassBooking.Application/**"
---

# Application Service Rules

Read `.github/skills/create-application-service/SKILL.md` before generating any service or interface code.

## Key rules (summary)

- Services live in `src/GymClassBooking.Application/Services/`. Always pair with an interface in `Interfaces/`.
- Inject `IAppDbContext` — never `AppDbContext` directly.
- Throw typed exceptions from `AppExceptions.cs` (`ClassFullException`, `BookingNotFoundException`, etc.) — never return nulls or booleans for business failures.
- No EF queries in controllers — all data access belongs here.
- Use `async`/`await` throughout. No `.Result` or `.Wait()`.
