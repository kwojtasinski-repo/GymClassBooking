---
name: create-waitlist-promotion
description: "Use when adding, modifying, or extending the waitlist promotion strategy — the rule that determines which waitlisted member gets a confirmed spot when another member cancels. Covers the current Premium-first FIFO implementation and how to extract IWaitlistStrategy for new promotion rules."
---

# Skill: Create Waitlist Promotion Strategy

Use this skill when changing **who gets promoted from the waitlist** when a confirmed booking is cancelled.

---

## Current implementation

Waitlist promotion is handled by two private methods in `BookingService`:

| Method                                       | Purpose                                                                                                                                                                         |
| -------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `PromoteFirstWaitlistedAsync(gymClassId)`    | Called from `CancelBookingAsync` when a confirmed booking is cancelled and the class is in the future. Picks the first member by priority and sets their status to `Confirmed`. |
| `GetWaitlistPosition(bookingId, gymClassId)` | Returns 1-based position of a booking in the priority-ordered waitlist. Used when returning `BookingResponse`.                                                                  |

**Current priority rule (in both methods):**

```csharp
.OrderByDescending(b => b.Member.MembershipTier) // Premium=1 before Standard=0
.ThenBy(b => b.BookedAt)                          // then FIFO
```

---

## When to modify inline (simple change)

If the change is a **minor tweak to the ordering** (e.g. change the tiebreaker from `BookedAt` to `Id`), just update the `OrderByDescending`/`ThenBy` chain in both `PromoteFirstWaitlistedAsync` and `GetWaitlistPosition`. They must stay in sync.

**Critical**: both methods must always use **identical ordering logic** — divergence causes `GetWaitlistPosition` to return wrong positions.

---

## When to extract IWaitlistStrategy (complex change)

If you need:

- Multiple swappable promotion rules
- Runtime configuration of the rule
- Testing the rule in isolation

### Step 1 — Define the interface in Application layer

```csharp
// src/GymClassBooking.Application/Interfaces/IWaitlistStrategy.cs
namespace GymClassBooking.Application.Interfaces;

public interface IWaitlistStrategy
{
    /// <summary>Returns waitlisted bookings sorted by promotion priority (first = next to promote).</summary>
    IOrderedEnumerable<Booking> Order(IEnumerable<Booking> waitlisted);
}
```

### Step 2 — Implement in Application layer

```csharp
// src/GymClassBooking.Application/Services/PremiumFirstWaitlistStrategy.cs
public class PremiumFirstWaitlistStrategy : IWaitlistStrategy
{
    public IOrderedEnumerable<Booking> Order(IEnumerable<Booking> waitlisted)
        => waitlisted
            .OrderByDescending(b => b.Member.MembershipTier)
            .ThenBy(b => b.BookedAt);
}
```

### Step 3 — Register in DependencyInjection.cs

```csharp
services.AddScoped<IWaitlistStrategy, PremiumFirstWaitlistStrategy>();
```

### Step 4 — Inject into BookingService

```csharp
public BookingService(IAppDbContext db, IWaitlistStrategy waitlistStrategy)
{
    _db = db;
    _waitlistStrategy = waitlistStrategy;
}
```

Replace the inline ordering in both private methods:

```csharp
var first = _waitlistStrategy.Order(waitlisted).FirstOrDefault();
```

---

## Rules

- The ordering logic in `PromoteFirstWaitlistedAsync` and `GetWaitlistPosition` **must always match**.
- Promotion only happens when `wasConfirmed && isInFuture` — do not change this guard.
- `MembershipTier.Premium = 1`, `Standard = 0` — descending order puts Premium first.
- Write a unit test for every new strategy variant. See `create-unit-test` skill.
