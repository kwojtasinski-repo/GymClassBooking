using GymClassBooking.Application.DTOs;
using GymClassBooking.Application.Exceptions;
using GymClassBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymClassBooking.API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> BookClass([FromBody] BookingRequest request)
    {
        try
        {
            var result = await _bookingService.BookClassAsync(request);
            return CreatedAtAction(nameof(BookClass), new { id = result.Id }, result);
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ConflictException ex) { return Conflict(new { error = ex.Message }); }
        catch (BusinessRuleException ex) { return UnprocessableEntity(new { error = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (BusinessRuleException ex) { return UnprocessableEntity(new { error = ex.Message }); }
    }
}
