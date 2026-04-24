namespace GymClassBooking.Domain.Entities;

public class GymClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxCapacity { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
