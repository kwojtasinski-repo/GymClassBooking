using GymClassBooking.Application.Interfaces;
using GymClassBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<GymClass> GymClasses => Set<GymClass>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.FullName).IsRequired().HasMaxLength(200);
            e.Property(m => m.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<GymClass>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.InstructorName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId);
            e.HasOne(b => b.GymClass)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.GymClassId);
        });
    }

    void IAppDbContext.Add<T>(T entity) => base.Add(entity);
    void IAppDbContext.Remove<T>(T entity) => base.Remove(entity);
}
