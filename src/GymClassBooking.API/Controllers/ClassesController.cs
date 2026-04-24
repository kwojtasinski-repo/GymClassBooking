using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymClassBooking.API.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassesController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public ClassesController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUpcomingClasses()
    {
        var classes = await _bookingService.GetUpcomingClassesAsync();
        return Ok(classes);
    }

    [HttpGet("{id:int}/bookings")]
    public async Task<IActionResult> GetClassBookings(int id)
    {
        try
        {
            var bookings = await _bookingService.GetClassBookingsAsync(id);
            return Ok(bookings);
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
