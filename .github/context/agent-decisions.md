# Agent Decisions Log

> **Append-only log of in-session corrections to AI agent behavior.**
> Operational, not architectural. For _why the system is built that way_ → ADRs in `docs/decisions.md`.
> For _how the agent should behave / what it missed / what to never repeat_ → here.
>
> **Read before non-trivial agent work** to avoid repeating past mistakes.
> **Append after any meaningful correction** that an agent received during a session.

> **SELF-CORRECTION TRIGGER** — If a user corrects your output (wrong logic, wrong layer, wrong pattern, wrong assumption):
> **Step 1** — append an entry here using the format below.
> **Step 2** — only then apply the fix.
> Do not skip step 1. Do not batch corrections. One mistake = one entry.

---

## When to write here vs. a design decision

| Situation                                                                   | Goes to              |
| --------------------------------------------------------------------------- | -------------------- |
| Architectural decision (technology, pattern, cross-cutting)                 | `docs/decisions.md`  |
| "Will I want to explain this to a new dev in a year?"                       | `docs/decisions.md`  |
| Agent missed a guard, forgot to read a context file, ignored an instruction | `agent-decisions.md` |
| Naming/format nit that the agent kept getting wrong                         | `agent-decisions.md` |
| Tool selection mistake (used wrong skill, wrong scope)                      | `agent-decisions.md` |
| Recurring drift you keep correcting in chat                                 | `agent-decisions.md` |

**Promotion rule**: if the same correction appears **2+ times** → promote to a permanent rule in `anti-patterns-critical.context.md`.

---

## Entry format (required)

```markdown
## YYYY-MM-DD — <area>

- **Context**: What the agent tried to do.
- **Decision**: What was decided instead.
- **Rationale**: Why — link to decisions.md entry or instructions.
- **Action**: What concrete change followed (file edited, rule added, etc.).
- **Promote?**: When does this graduate to anti-patterns-critical ("after 2nd occurrence").
- **Status**: Open | Resolved | Promoted → <ref>
```

Rules: one H2 per entry. **Append, do not edit history**. Date in `YYYY-MM-DD`. Keep entries to 5–10 lines.

---

## Entries

<!-- Append new entries below this line, newest at the bottom. -->

## 2026-04-25 — Bookings / Application layer

- **Context**: Agent calculated waitlist position directly in `BookingsController`, iterating over the waitlist query inside the action method.
- **Decision**: All waitlist position logic must live in `BookingService` (Application layer). Controllers only call service methods.
- **Rationale**: Business logic in controllers violates layered architecture — see `copilot-instructions.md`.
- **Action**: Moved `GetWaitlistPosition` helper and ordering logic into `BookingService`. Controller receives position inside `BookingResponse` DTO.
- **Promote?**: After 2nd occurrence → `anti-patterns-critical.context.md` ("No business logic in controllers").
- **Status**: Resolved

---

## 2026-04-25 — Bookings / Capacity check

- **Context**: Agent used `b.Status != BookingStatus.Cancelled` for the capacity count — included Waitlisted bookings and blocked new confirmed slots incorrectly.
- **Decision**: Capacity check must filter `b.Status == BookingStatus.Confirmed` only. Waitlisted bookings do not occupy a confirmed slot.
- **Rationale**: `docs/decisions.md D-001` — confirmed count is the canonical capacity check.
- **Action**: Fixed `CountAsync` predicate in `BookingService.BookClassAsync`.
- **Promote?**: Promoted.
- **Status**: Promoted → docs/decisions.md D-001

---

## 2026-04-25 — Infrastructure / Database provider

- **Context**: Agent set up SQLite with file-based persistence and EF Core migrations. Caused dual-provider conflicts when `WebApplicationFactory` tried to replace SQLite with InMemory in integration tests.
- **Decision**: `AddInfrastructure()` unconditionally calls `UseInMemoryDatabase("GymClassBooking")`. No migrations, no connection strings, no override hacks. Integration tests use `IAsyncLifetime.InitializeAsync`: `EnsureDeletedAsync` → `EnsureCreatedAsync` → `DbSeeder.SeedAsync`.
- **Rationale**: Simpler DI, zero schema drift, no dual-provider conflict — see `docs/decisions.md D-004`.
- **Action**: Replaced Sqlite package with `Microsoft.EntityFrameworkCore.InMemory`, removed Migrations folder, simplified `DependencyInjection.cs`.
- **Promote?**: Promoted.
- **Status**: Promoted → docs/decisions.md D-004

---

## 2026-04-25 — API / OpenAPI provider

- **Context**: Agent used Swashbuckle.AspNetCore 10.x. Its dependency on `Microsoft.OpenApi` 2.x caused `ReflectionTypeLoadException` in integration tests when `WebApplicationFactory` scanned the API assembly.
- **Decision**: Use `AddOpenApi()` + `MapOpenApi()` from the built-in `Microsoft.AspNetCore.OpenApi` package. Do not add Swashbuckle.
- **Rationale**: Eliminates assembly scanning conflict, aligns with .NET 10 native API docs approach.
- **Action**: Removed Swashbuckle package; replaced `AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI` with `AddOpenApi`/`MapOpenApi`.
- **Promote?**: After 2nd occurrence → `anti-patterns-critical.context.md`.
- **Status**: Resolved

---

## 2026-04-26 — Bookings / decisions.md amendment

- **Context**: Agent received a prompt framed as "fix the inconsistency" between the LateCancel penalty scope (Standard-only) and the booking block (all tiers). Agent amended existing D-003 entry in `docs/decisions.md` to retroactively justify its implementation, instead of creating a new decision entry.
- **Decision**: `docs/decisions.md` is append-only. When a prompt conflicts with an Active decision, the agent must surface the conflict to the user before writing code or creating a new D-entry. Never amend existing entries.
- **Rationale**: Amending an ADR to match implementation corrupts the rationale record for all future agents and developers — see `anti-patterns-critical.context.md` rule 9.
- **Action**: Added rule 9 to `anti-patterns-critical.context.md`. Added append-only + conflict-surfacing rule to `copilot-instructions.md`. Strengthened D-003 Decision text to make penalty vs. block distinction explicit, eliminating the apparent inconsistency the agent saw.
- **Promote?**: Promoted immediately — occurred 3 times in one session.
- **Status**: Promoted → `anti-patterns-critical.context.md` rule 9
