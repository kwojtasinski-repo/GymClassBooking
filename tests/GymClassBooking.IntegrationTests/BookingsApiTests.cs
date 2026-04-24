using GymClassBooking.Infrastructure.Persistence;
using GymClassBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace GymClassBooking.IntegrationTests;

/// <summary>
/// Integration tests use the same InMemory database that AddInfrastructure registers.
/// IAsyncLifetime resets and re-seeds before each test to guarantee isolation.
/// </summary>
public class BookingsApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;

    public BookingsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUpcomingClasses_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/classes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var classes = await response.Content.ReadFromJsonAsync<List<GymClassSummary>>();
        Assert.NotNull(classes);
        Assert.NotEmpty(classes!);
    }

    [Fact]
    public async Task PostBooking_ValidRequest_ReturnsCreated()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pilates = db.GymClasses.First(c => c.Name == "Pilates Core");
        var carol = db.Members.First(m => m.FullName == "Carol White");

        var response = await _client.PostAsJsonAsync("/api/bookings",
            new { memberId = carol.Id, gymClassId = pilates.Id });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
        Assert.NotNull(booking);
        Assert.Equal("Confirmed", booking!.Status);
    }

    [Fact]
    public async Task PostBooking_DuplicateBooking_ReturnsConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alice = db.Members.First(m => m.FullName == "Alice Johnson");
        var hiit = db.GymClasses.First(c => c.Name == "HIIT Blast");

        // Alice is already booked in HIIT from seed data
        var response = await _client.PostAsJsonAsync("/api/bookings",
            new { memberId = alice.Id, gymClassId = hiit.Id });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBooking_ValidId_ReturnsNoContent()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = db.Bookings.First(b => b.Status == GymClassBooking.Domain.Enums.BookingStatus.Confirmed);

        var response = await _client.DeleteAsync($"/api/bookings/{booking.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
