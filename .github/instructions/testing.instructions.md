---
description: "Use when adding or modifying unit tests, integration tests, test fixtures, test helpers, or anything in a test project. Covers Guid-named InMemory DB pattern for unit tests, IAsyncLifetime + WebApplicationFactory pattern for integration tests, and test isolation rules for this project."
applyTo: "tests/**"
---

# Testing Rules

For **unit tests**: read `.github/skills/create-unit-test/SKILL.md`.  
For **integration tests**: read `.github/skills/create-integration-test/SKILL.md`.

## Key rules (summary)

**Unit tests** (`GymClassBooking.UnitTests`):

- Create a fresh `AppDbContext` per test using a unique `Guid.NewGuid().ToString()` InMemory database name.
- Seed only what the specific test needs — do not call `DbSeeder.SeedAsync`.
- No mocking of `DbContext`.

**Integration tests** (`GymClassBooking.IntegrationTests`):

- Use `IClassFixture<WebApplicationFactory<Program>>` + `IAsyncLifetime`.
- `InitializeAsync`: `EnsureDeletedAsync` → `EnsureCreatedAsync` → `DbSeeder.SeedAsync` — always in that order.
- Never add a second EF Core provider — use the InMemory provider registered by `AddInfrastructure` as-is.
- Never use `IClassFixture` for DB state — use `IAsyncLifetime` so each test gets a clean database.
