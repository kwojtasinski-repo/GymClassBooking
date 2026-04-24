using GymClassBooking.Domain.Entities;
using GymClassBooking.Domain.Enums;

namespace GymClassBooking.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Members.Any()) return;

        var now = DateTime.UtcNow;

        var members = new List<Member>
        {
            new() { FullName = "Alice Johnson", Email = "alice@example.com", MembershipTier = MembershipTier.Premium },
            new() { FullName = "Bob Smith", Email = "bob@example.com", MembershipTier = MembershipTier.Standard },
            new() { FullName = "Carol White", Email = "carol@example.com", MembershipTier = MembershipTier.Premium },
            new() { FullName = "Dave Brown", Email = "dave@example.com", MembershipTier = MembershipTier.Standard }
        };

        db.Members.AddRange(members);

        var classes = new List<GymClass>
        {
            new() { Name = "Morning Yoga", InstructorName = "Sarah Lee", StartsAt = now.AddDays(1).Date.AddHours(8), DurationMinutes = 60, MaxCapacity = 2 },
            new() { Name = "HIIT Blast", InstructorName = "Mike Torres", StartsAt = now.AddDays(2).Date.AddHours(18), DurationMinutes = 45, MaxCapacity = 10 },
            new() { Name = "Pilates Core", InstructorName = "Emma Davis", StartsAt = now.AddDays(3).Date.AddHours(10), DurationMinutes = 50, MaxCapacity = 8 },
            new() { Name = "Spin Class", InstructorName = "Jake Wilson", StartsAt = now.AddDays(5).Date.AddHours(7), DurationMinutes = 45, MaxCapacity = 12 },
            new() { Name = "Boxing Fundamentals", InstructorName = "Liam Carter", StartsAt = now.AddDays(7).Date.AddHours(17), DurationMinutes = 60, MaxCapacity = 6 }
        };

        db.GymClasses.AddRange(classes);
        await db.SaveChangesAsync();

        // Seed bookings: Morning Yoga (capacity=2) — fill it and add a waitlisted booking
        var yoga = classes[0];
        var alice = members[0]; // Premium
        var bob = members[1];   // Standard
        var carol = members[2]; // Premium — will be waitlisted but gets priority
        var dave = members[3];  // Standard

        var bookings = new List<Booking>
        {
            // Yoga is full (2 confirmed)
            new() { MemberId = alice.Id, GymClassId = yoga.Id, BookedAt = now.AddHours(-3), Status = BookingStatus.Confirmed },
            new() { MemberId = bob.Id, GymClassId = yoga.Id, BookedAt = now.AddHours(-2), Status = BookingStatus.Confirmed },
            // Carol (Premium) is waitlisted — position 1 due to Premium priority
            new() { MemberId = carol.Id, GymClassId = yoga.Id, BookedAt = now.AddHours(-1), Status = BookingStatus.Waitlisted },
            // HIIT — Alice and Dave confirmed
            new() { MemberId = alice.Id, GymClassId = classes[1].Id, BookedAt = now.AddHours(-5), Status = BookingStatus.Confirmed },
            new() { MemberId = dave.Id, GymClassId = classes[1].Id, BookedAt = now.AddHours(-4), Status = BookingStatus.Confirmed },
        };

        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
