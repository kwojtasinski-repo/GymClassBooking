using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Services;
using GymClassBooking.Domain.Entities;
using GymClassBooking.Domain.Enums;
using GymClassBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.UnitTests;

public class BookingServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new BookingService(_db);
    }

    private async Task<(Member member, GymClass gymClass)> SeedBasicAsync(
        MembershipTier tier = MembershipTier.Standard, int capacity = 5)
    {
        var member = new Member { FullName = "Test User", Email = $"{Guid.NewGuid()}@test.com", MembershipTier = tier };
        var gymClass = new GymClass
        {
            Name = "Test Class",
            InstructorName = "Instructor",
            StartsAt = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 60,
            MaxCapacity = capacity
        };
        _db.Members.Add(member);
        _db.GymClasses.Add(gymClass);
        await _db.SaveChangesAsync();
        return (member, gymClass);
    }

    [Fact]
    public async Task BookClass_WhenSpotAvailable_ReturnsConfirmedStatus()
    {
        var (member, gymClass) = await SeedBasicAsync();

        var result = await _service.BookClassAsync(new(member.Id, gymClass.Id));

        Assert.Equal("Confirmed", result.Status);
        Assert.Null(result.WaitlistPosition);
    }

    [Fact]
    public async Task BookClass_WhenClassFull_ReturnsWaitlistedStatus()
    {
        var (member, gymClass) = await SeedBasicAsync(capacity: 1);

        // Fill the class
        var member2 = new Member { FullName = "Other", Email = "other@test.com", MembershipTier = MembershipTier.Standard };
        _db.Members.Add(member2);
        await _db.SaveChangesAsync();
        await _service.BookClassAsync(new(member2.Id, gymClass.Id));

        // Now book when full
        var result = await _service.BookClassAsync(new(member.Id, gymClass.Id));

        Assert.Equal("Waitlisted", result.Status);
        Assert.Equal(1, result.WaitlistPosition);
    }

    [Fact]
    public async Task BookClass_DuplicateBooking_ThrowsConflictException()
    {
        var (member, gymClass) = await SeedBasicAsync();
        await _service.BookClassAsync(new(member.Id, gymClass.Id));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.BookClassAsync(new(member.Id, gymClass.Id)));
    }

    [Fact]
    public async Task BookClass_MemberWithLateCancel_ThrowsBusinessRuleException()
    {
        var (member, gymClass) = await SeedBasicAsync();
        member.LateCancel = true;
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.BookClassAsync(new(member.Id, gymClass.Id)));
    }

    [Fact]
    public async Task CancelBooking_ConfirmedBookingWithWaitlist_PromotesWaitlistedBooking()
    {
        var (member, gymClass) = await SeedBasicAsync(capacity: 1);

        var member2 = new Member { FullName = "Waitlister", Email = "w@test.com", MembershipTier = MembershipTier.Standard };
        _db.Members.Add(member2);
        await _db.SaveChangesAsync();

        var confirmed = await _service.BookClassAsync(new(member.Id, gymClass.Id));
        var waitlisted = await _service.BookClassAsync(new(member2.Id, gymClass.Id));

        Assert.Equal("Waitlisted", waitlisted.Status);

        await _service.CancelBookingAsync(confirmed.Id);

        var promoted = await _db.Bookings.FindAsync(waitlisted.Id);
        Assert.Equal(BookingStatus.Confirmed, promoted!.Status);
    }

    [Fact]
    public async Task CancelBooking_LateCancel_StandardMemberGetsFlag()
    {
        var (member, gymClass) = await SeedBasicAsync();

        // Move class start to be within 2 hours
        gymClass.StartsAt = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        var booking = await _service.BookClassAsync(new(member.Id, gymClass.Id));
        await _service.CancelBookingAsync(booking.Id);

        var updatedMember = await _db.Members.FindAsync(member.Id);
        Assert.True(updatedMember!.LateCancel);
    }

    [Fact]
    public async Task CancelBooking_LateCancel_PremiumMemberNotFlagged()
    {
        var (member, gymClass) = await SeedBasicAsync(tier: MembershipTier.Premium);
        gymClass.StartsAt = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        var booking = await _service.BookClassAsync(new(member.Id, gymClass.Id));
        await _service.CancelBookingAsync(booking.Id);

        var updatedMember = await _db.Members.FindAsync(member.Id);
        Assert.False(updatedMember!.LateCancel);
    }

    [Fact]
    public async Task WaitlistOrder_PremiumMembersBeforeStandard()
    {
        var gymClass = new GymClass
        {
            Name = "Priority Test",
            InstructorName = "I",
            StartsAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 45,
            MaxCapacity = 1
        };
        _db.GymClasses.Add(gymClass);

        var filler = new Member { FullName = "Filler", Email = "f@test.com", MembershipTier = MembershipTier.Standard };
        var standardWaiter = new Member { FullName = "Standard", Email = "s@test.com", MembershipTier = MembershipTier.Standard };
        var premiumWaiter = new Member { FullName = "Premium", Email = "p@test.com", MembershipTier = MembershipTier.Premium };
        _db.Members.AddRange(filler, standardWaiter, premiumWaiter);
        await _db.SaveChangesAsync();

        // Fill the class
        await _service.BookClassAsync(new(filler.Id, gymClass.Id));

        // Standard waits first, then Premium
        var standardBooking = await _service.BookClassAsync(new(standardWaiter.Id, gymClass.Id));
        var premiumBooking = await _service.BookClassAsync(new(premiumWaiter.Id, gymClass.Id));

        Assert.Equal("Waitlisted", standardBooking.Status);
        Assert.Equal("Waitlisted", premiumBooking.Status);

        // After both are waitlisted, Premium should be at position 1
        // Verify by getting class bookings (which re-computes positions)
        var classBookings = await _service.GetClassBookingsAsync(gymClass.Id);
        var premiumEntry = classBookings.First(b => b.MemberId == premiumWaiter.Id);
        var standardEntry = classBookings.First(b => b.MemberId == standardWaiter.Id);

        Assert.Equal(1, premiumEntry.WaitlistPosition);
        Assert.Equal(2, standardEntry.WaitlistPosition);
    }

    public void Dispose() => _db.Dispose();
}
