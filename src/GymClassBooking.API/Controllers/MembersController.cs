using GymClassBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymClassBooking.API.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IAppDbContext _db;

    public MembersController(IBookingService bookingService, IAppDbContext db)
    {
        _bookingService = bookingService;
        _db = db;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (member is null) return NotFound(new { error = $"Member {id} not found." });

        return Ok(new
        {
            member.Id,
            member.FullName,
            member.Email,
            MembershipTier = member.MembershipTier.ToString(),
            member.LateCancel
        });
    }

    [HttpGet("{id:int}/bookings")]
    public async Task<IActionResult> GetMemberBookings(int id)
    {
        var bookings = await _bookingService.GetMemberBookingsAsync(id);
        return Ok(bookings);
    }
}
