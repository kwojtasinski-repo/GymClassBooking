# GymClassBooking — Design Decisions

Entries newest-first.

---

## [D-005] Blazor WASM is the frontend — Blazor Server is not used

**Status:** `Active`
**Area:** Frontend

### Decision

The web frontend is Blazor WebAssembly (WASM). It runs entirely in the browser and communicates with the API over HTTP. Blazor Server (SignalR-based) is not used.

### Alternatives considered

- ~~Blazor Server — lower initial download, but requires persistent SignalR connection and server-side state; less suitable for a potentially auto-scaled API backend~~
- **Blazor WASM ✅** — chosen because it is fully decoupled from the backend, works as a static site, and can be hosted on a CDN

### Consequences

- ✅ API and frontend are independently deployable
- ✅ CORS must be configured on the API (`"BlazorClient"` policy) — allowed origins: `https://localhost:7185`, `http://localhost:5042`
- ⚠️ Initial page load downloads the .NET runtime to the browser (~4 MB)
- ⚠️ No direct server-side rendering — all data fetched via `HttpClient`

---

## [D-004] EF Core InMemory — no SQLite, no migrations

**Status:** `Active`
**Area:** Infrastructure

### Decision

`AddInfrastructure()` (no parameters) unconditionally registers `UseInMemoryDatabase("GymClassBooking")`. There is no SQLite, no file-based persistence, and no EF Core migrations in this project. `DbSeeder.SeedAsync` is called from `Program.cs` on every startup to populate the in-memory store.

### Alternatives considered

- ~~SQLite file-based with migrations — caused dual-provider conflicts in `WebApplicationFactory` integration tests when tests tried to swap to InMemory~~
- ~~SQLite in-memory (`:memory:`) as a test-only override — worked but required a custom `TestWebApplicationFactory` and a shared `SqliteConnection` to keep the schema alive~~
- **EF Core InMemory unconditionally ✅** — simplest DI registration; integration tests share the same provider; `IAsyncLifetime.InitializeAsync` resets and re-seeds before each test

### Consequences

- ✅ Zero schema drift; no migration management
- ✅ Integration tests use identical DI registration as production — no overrides
- ⚠️ Data does not persist across restarts — this is intentional for a demo/dev project
- ⚠️ InMemory does not enforce relational constraints (FK, unique index) — business rules must be enforced in service code

---

## [D-003] Late cancellation penalty applies to Standard members only

**Status:** `Active`
**Area:** Bookings

### Decision

When a member cancels a booking less than 2 hours before class start, the `LateCancel` flag is set on the `Member` entity only if their `MembershipTier` is `Standard`. Premium members are exempt from **earning** this penalty. However, once the flag is set by any means, the booking block in `BookClassAsync` applies to **all tiers** — the block is a staff control mechanism, not an extension of the penalty. Only staff can clear the flag; no code path auto-clears it.

### Alternatives considered

- ~~All members penalised equally — too punitive for premium subscribers who pay more~~
- ~~Block only Standard members (mirror the penalty scope) — would silently bypass the staff control mechanism for Premium members if the flag were ever set via data migration or admin tool~~
- **Standard-only penalty, tier-blind block ✅** — chosen because the penalty and the block serve different purposes: the penalty differentiates tiers, the block is a staff intervention gate

### Consequences

- ✅ Clear business rule that aligns with tiered membership model
- ✅ Staff retain full control via the flag regardless of membership tier
- ⚠️ Staff must have a mechanism to clear the `LateCancel` flag (not yet exposed as an API endpoint — future work)

---

## [D-002] Waitlist priority: Premium before Standard, then FIFO within tier

**Status:** `Active`
**Area:** Bookings

### Decision

When a class is full and a booking becomes `Waitlisted`, the position in the waitlist is determined by two factors: first, membership tier (Premium = higher priority than Standard), then by `BookedAt` ascending (FIFO) within the same tier. This ordering is computed on-the-fly each time waitlist positions are exposed.

### Alternatives considered

- ~~Pure FIFO regardless of tier — ignores Premium membership value~~
- **Tier-priority + FIFO ✅** — chosen because it rewards Premium members with tangible queue benefits

### Consequences

- ✅ Premium members get real benefit from their tier
- ⚠️ A Standard member who waits a long time can be pushed back if Premium members join later — acceptable trade-off by design

---

## [D-001] Booking capacity check uses confirmed bookings count only

**Status:** `Active`
**Area:** Bookings

### Decision

When determining if a class has space, only bookings with `Status = Confirmed` count toward `MaxCapacity`. Waitlisted and cancelled bookings are excluded from the count. This ensures the confirmed slot count never exceeds capacity.

### Alternatives considered

- ~~Count all non-cancelled bookings — would block new confirmations when there are waitlisted spots~~
- **Confirmed-only count ✅** — chosen because it is the accurate representation of class occupancy

### Consequences

- ✅ Capacity is never exceeded by confirmed attendees
- ✅ Waitlist can grow independently of confirmed count
