# docs/

This folder contains human-readable project documentation.

## Files

| File | Purpose |
|---|---|
| [decisions.md](decisions.md) | Design decisions (ADRs). Read before modifying business logic. Entries newest-first. |

## For agent/copilot context files

Agent-facing context (corrections, decisions for AI) lives in `.github/context/`:

| File | Purpose |
|---|---|
| `.github/context/agent-decisions.md` | Agent correction log — scan before every non-trivial task. |

## Design decision index

| ID | Area | Decision |
|---|---|---|
| D-005 | Frontend | Blazor WASM — not Blazor Server |
| D-004 | Infrastructure | EF Core InMemory — no SQLite, no migrations |
| D-003 | Bookings | Late cancellation penalty for Standard members only |
| D-002 | Bookings | Waitlist priority: Premium first, then FIFO |
| D-001 | Bookings | Capacity check counts Confirmed bookings only |
