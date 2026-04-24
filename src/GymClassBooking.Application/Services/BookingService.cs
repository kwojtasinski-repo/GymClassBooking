using GymClassBooking.Application.DTOs;
using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Interfaces;
using GymClassBooking.Domain.Entities;
using GymClassBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _db;

    public BookingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<BookingResponse> BookClassAsync(BookingRequest request)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId)
            ?? throw new NotFoundException($"Member {request.MemberId} not found.");

        if (member.LateCancel)
            throw new BusinessRuleException("Member has a late cancellation flag and cannot make new bookings until cleared by staff.");

        var gymClass = await _db.GymClasses
            .FirstOrDefaultAsync(c => c.Id == request.GymClassId)
            ?? throw new NotFoundException($"GymClass {request.GymClassId} not found.");

        var existingBooking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.MemberId == request.MemberId &&
                b.GymClassId == request.GymClassId &&
                b.Status != BookingStatus.Cancelled);

        if (existingBooking is not null)
            throw new ConflictException("Member already has an active booking for this class.");

        var confirmedCount = await _db.Bookings
            .CountAsync(b => b.GymClassId == request.GymClassId && b.Status == BookingStatus.Confirmed);

        BookingStatus status;
        if (confirmedCount < gymClass.MaxCapacity)
        {
            status = BookingStatus.Confirmed;
        }
        else
        {
            status = BookingStatus.Waitlisted;
        }

        var booking = new Booking
        {
            MemberId = request.MemberId,
            GymClassId = request.GymClassId,
            BookedAt = DateTime.UtcNow,
            Status = status
        };

        _db.Add(booking);
        await _db.SaveChangesAsync();

        int? waitlistPosition = null;
        if (status == BookingStatus.Waitlisted)
        {
            waitlistPosition = await GetWaitlistPosition(booking.Id, request.GymClassId);
        }

        return new BookingResponse(
            booking.Id,
            member.Id,
            member.FullName,
            gymClass.Id,
            gymClass.Name,
            booking.BookedAt,
            booking.Status.ToString(),
            waitlistPosition);
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Member)
            .Include(b => b.GymClass)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var isInFuture = booking.GymClass.StartsAt > DateTime.UtcNow;
        var isLateCancellation = booking.GymClass.StartsAt - DateTime.UtcNow < TimeSpan.FromHours(2);

        // Apply late cancellation penalty to Standard members only
        if (isLateCancellation && booking.Member.MembershipTier == MembershipTier.Standard)
        {
            booking.Member.LateCancel = true;
        }

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();

        // Promote first waitlisted booking if cancelled was confirmed and class is in the future
        if (wasConfirmed && isInFuture)
        {
            await PromoteFirstWaitlistedAsync(booking.GymClassId);
        }
    }

    public async Task<IEnumerable<GymClassSummary>> GetUpcomingClassesAsync()
    {
        var now = DateTime.UtcNow;
        var classes = await _db.GymClasses
            .Where(c => c.StartsAt > now)
            .OrderBy(c => c.StartsAt)
            .ToListAsync();

        var result = new List<GymClassSummary>();
        foreach (var c in classes)
        {
            var confirmed = await _db.Bookings.CountAsync(b => b.GymClassId == c.Id && b.Status == BookingStatus.Confirmed);
            var waitlisted = await _db.Bookings.CountAsync(b => b.GymClassId == c.Id && b.Status == BookingStatus.Waitlisted);
            result.Add(new GymClassSummary(c.Id, c.Name, c.InstructorName, c.StartsAt, c.DurationMinutes, c.MaxCapacity, confirmed, waitlisted));
        }

        return result;
    }

    public async Task<IEnumerable<BookingResponse>> GetClassBookingsAsync(int gymClassId)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Member)
            .Include(b => b.GymClass)
            .Where(b => b.GymClassId == gymClassId && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.Status)
            .ThenBy(b => b.BookedAt)
            .ToListAsync();

        var result = new List<BookingResponse>();
        foreach (var b in bookings)
        {
            int? pos = b.Status == BookingStatus.Waitlisted
                ? await GetWaitlistPosition(b.Id, gymClassId)
                : null;

            result.Add(MapToResponse(b, pos));
        }

        return result;
    }

    public async Task<IEnumerable<MemberBookingResponse>> GetMemberBookingsAsync(int memberId)
    {
        var bookings = await _db.Bookings
            .Include(b => b.GymClass)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();

        var result = new List<MemberBookingResponse>();
        foreach (var b in bookings)
        {
            int? pos = b.Status == BookingStatus.Waitlisted
                ? await GetWaitlistPosition(b.Id, b.GymClassId)
                : null;

            result.Add(new MemberBookingResponse(
                b.Id,
                b.GymClassId,
                b.GymClass.Name,
                b.GymClass.StartsAt,
                b.BookedAt,
                b.Status.ToString(),
                pos));
        }

        return result;
    }

    private async Task PromoteFirstWaitlistedAsync(int gymClassId)
    {
        // Load all waitlisted bookings with member info for priority ordering
        var waitlisted = await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.GymClassId == gymClassId && b.Status == BookingStatus.Waitlisted)
            .ToListAsync();

        // Premium before Standard, then FIFO within tier
        var first = waitlisted
            .OrderByDescending(b => b.Member.MembershipTier) // Premium=1 > Standard=0
            .ThenBy(b => b.BookedAt)
            .FirstOrDefault();

        if (first is not null)
        {
            first.Status = BookingStatus.Confirmed;
            await _db.SaveChangesAsync();
        }
    }

    private async Task<int> GetWaitlistPosition(int bookingId, int gymClassId)
    {
        // Load all waitlisted bookings for this class with member info
        var waitlisted = await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.GymClassId == gymClassId && b.Status == BookingStatus.Waitlisted)
            .ToListAsync();

        // Sort by priority: Premium before Standard, then FIFO
        var ordered = waitlisted
            .OrderByDescending(b => b.Member.MembershipTier)
            .ThenBy(b => b.BookedAt)
            .ToList();

        var index = ordered.FindIndex(b => b.Id == bookingId);
        return index >= 0 ? index + 1 : -1;
    }

    private static BookingResponse MapToResponse(Booking b, int? waitlistPosition) =>
        new(b.Id, b.MemberId, b.Member.FullName, b.GymClassId, b.GymClass.Name, b.BookedAt, b.Status.ToString(), waitlistPosition);
}
