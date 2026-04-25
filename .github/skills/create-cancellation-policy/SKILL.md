---
name: create-cancellation-policy
description: "Use when adding, modifying, or extending the late cancellation penalty rule — the business rule that determines whether a member receives a LateCancel flag when they cancel a booking. Covers the current 2-hour window Standard-only rule and how to extract ICancellationPolicy for new penalty variants."
---

# Skill: Create Cancellation Policy

Use this skill when changing **what happens to a member when they cancel a booking late**.

---

## Current implementation

The cancellation penalty lives in `BookingService.CancelBookingAsync`:

```csharp
var isLateCancellation = booking.GymClass.StartsAt - DateTime.UtcNow < TimeSpan.FromHours(2);

// Apply late cancellation penalty to Standard members only
if (isLateCancellation && booking.Member.MembershipTier == MembershipTier.Standard)
{
    booking.Member.LateCancel = true;
}
```

**Current rule:**

- Window: less than 2 hours before class starts
- Applies to: `MembershipTier.Standard` only
- Penalty: sets `Member.LateCancel = true`
- Effect: `BookClassAsync` blocks any new booking by a member with `LateCancel = true` until staff clears it

---

## When to modify inline (simple change)

Changing the time window or applying to all tiers — edit the two conditions directly in `CancelBookingAsync`. Keep the logic co-located.

---

## When to extract ICancellationPolicy (complex change)

If you need multiple policies, runtime configuration, or per-tier rules:

### Step 1 — Define the interface in Application layer

```csharp
// src/GymClassBooking.Application/Interfaces/ICancellationPolicy.cs
namespace GymClassBooking.Application.Interfaces;

public interface ICancellationPolicy
{
    /// <summary>Apply penalty side-effects to the member. Called before SaveChanges.</summary>
    void Apply(Member member, GymClass gymClass, DateTime cancelledAt);
}
```

### Step 2 — Implement

```csharp
// src/GymClassBooking.Application/Services/StandardLateCancelPolicy.cs
public class StandardLateCancelPolicy : ICancellationPolicy
{
    private static readonly TimeSpan Window = TimeSpan.FromHours(2);

    public void Apply(Member member, GymClass gymClass, DateTime cancelledAt)
    {
        var isLate = gymClass.StartsAt - cancelledAt < Window;
        if (isLate && member.MembershipTier == MembershipTier.Standard)
            member.LateCancel = true;
    }
}
```

### Step 3 — Register in DependencyInjection.cs

```csharp
services.AddScoped<ICancellationPolicy, StandardLateCancelPolicy>();
```

### Step 4 — Inject into BookingService

```csharp
_cancellationPolicy.Apply(booking.Member, booking.GymClass, DateTime.UtcNow);
```

---

## Rules

- Penalty is applied **before** `SaveChangesAsync` — the flag and the status change are committed atomically.
- The `LateCancel` flag blocks ALL future bookings until cleared by staff — this is intentional.
- Clearing `LateCancel` is a staff-only operation — do not add auto-clear logic here.

- Write a unit test for every new policy variant. See `create-unit-test` skill.
