---
name: create-booking-rule
description: "Use when adding, modifying, or extending the booking flow — the sequence of guards and checks in BookClassAsync. Covers the required guard order (member exists, late-cancel block, class exists, duplicate check, capacity check, waitlist placement) and how to safely add new booking rules without breaking existing guards."
---

# Skill: Create Booking Rule

Use this skill when adding a new guard or condition to the **booking flow** in `BookingService.BookClassAsync`.

---

## Current booking flow (guard order is critical)

```
1. Load member              → NotFoundException if not found
2. LateCancel check         → BusinessRuleException if member.LateCancel = true — applies to ALL tiers by design (the block is a staff control, not part of the penalty; see D-003 in docs/decisions.md)
3. Load gym class           → NotFoundException if not found
4. Duplicate booking check  → ConflictException if active booking exists
5. Capacity check           → Confirmed if slots available, else Waitlisted
6. Save booking
7. Calculate waitlist position (if Waitlisted)
8. Return BookingResponse
```

**Do not reorder these guards.** Specifically:

- Member must be loaded before LateCancel check (step 2 needs step 1).
- Class must be loaded before capacity check (step 5 needs step 3).
- Duplicate check (step 4) must come before capacity check — a duplicate should always be `Conflict`, never `Waitlisted`.

---

## Adding a new guard

### Pattern — insert after the relevant load step

```csharp
// Example: block bookings for members with outstanding fees
var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId)
    ?? throw new NotFoundException(...);

// NEW GUARD — insert here, after member is loaded
if (member.HasOutstandingFees)
    throw new BusinessRuleException("Cannot book while account has outstanding fees.");
```

### Exception types

| Situation                  | Exception               | HTTP result in controller |
| -------------------------- | ----------------------- | ------------------------- |
| Entity not found           | `NotFoundException`     | `404 NotFound`            |
| Duplicate / already exists | `ConflictException`     | `409 Conflict`            |
| Business rule violated     | `BusinessRuleException` | `422 UnprocessableEntity` |

All exception types are in `src/GymClassBooking.Application/Exceptions/AppExceptions.cs`.

---

## Capacity check rule

```csharp
var confirmedCount = await _db.Bookings
    .CountAsync(b => b.GymClassId == request.GymClassId && b.Status == BookingStatus.Confirmed);
```

**Count only `Confirmed` bookings** — `Waitlisted` bookings do not occupy a slot (D-001 in `docs/decisions.md`).

---

## Waitlist placement

When `confirmedCount >= gymClass.MaxCapacity`, the new booking is `Waitlisted`. Position is calculated by `GetWaitlistPosition` using the same ordering as `PromoteFirstWaitlistedAsync`.

To change who gets priority on the waitlist, see `create-waitlist-promotion` skill.

---

## Rules

- All guards throw typed exceptions — never return `null` or `bool` for a failure.
- All guards run before `_db.Add(booking)` — no partial saves.
- The `BookingResponse` must always include `WaitlistPosition` when `Status == Waitlisted`.
- Adding a new guard requires a new unit test covering the blocked case. See `create-unit-test` skill.
- If the new guard encodes a significant business decision, add a `docs/decisions.md` entry.
