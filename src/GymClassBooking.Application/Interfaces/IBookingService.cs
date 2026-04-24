using GymClassBooking.Application.DTOs;

namespace GymClassBooking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> BookClassAsync(BookingRequest request);
    Task CancelBookingAsync(int bookingId);
    Task<IEnumerable<GymClassSummary>> GetUpcomingClassesAsync();
    Task<IEnumerable<BookingResponse>> GetClassBookingsAsync(int gymClassId);
    Task<IEnumerable<MemberBookingResponse>> GetMemberBookingsAsync(int memberId);
}
