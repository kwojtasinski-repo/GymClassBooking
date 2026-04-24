using GymClassBooking.Domain.Enums;

namespace GymClassBooking.Domain.Entities;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MembershipTier MembershipTier { get; set; }
    public bool LateCancel { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
