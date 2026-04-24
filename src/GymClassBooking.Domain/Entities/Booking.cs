using GymClassBooking.Domain.Enums;

namespace GymClassBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int GymClassId { get; set; }
    public DateTime BookedAt { get; set; }
    public BookingStatus Status { get; set; }

    public Member Member { get; set; } = null!;
    public GymClass GymClass { get; set; } = null!;
}
