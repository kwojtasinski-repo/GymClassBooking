using GymClassBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Member> Members { get; }
    DbSet<GymClass> GymClasses { get; }
    DbSet<Booking> Bookings { get; }

    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
