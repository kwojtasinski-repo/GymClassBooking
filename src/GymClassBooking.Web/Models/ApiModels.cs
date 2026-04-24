namespace GymClassBooking.Web.Models;

public record GymClassSummary(
    int Id,
    string Name,
    string InstructorName,
    DateTime StartsAt,
    int DurationMinutes,
    int MaxCapacity,
    int ConfirmedCount,
    int WaitlistCount);

public record BookingResponse(
    int Id,
    int MemberId,
    string MemberName,
    int GymClassId,
    string GymClassName,
    DateTime BookedAt,
    string Status,
    int? WaitlistPosition);

public record MemberBookingResponse(
    int Id,
    int GymClassId,
    string GymClassName,
    DateTime StartsAt,
    DateTime BookedAt,
    string Status,
    int? WaitlistPosition);

public record MemberResponse(
    int Id,
    string FullName,
    string Email,
    string MembershipTier,
    bool LateCancel);

public record BookingRequest(int MemberId, int GymClassId);
