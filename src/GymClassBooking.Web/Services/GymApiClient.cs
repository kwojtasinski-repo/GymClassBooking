using GymClassBooking.Web.Models;
using System.Net.Http.Json;

namespace GymClassBooking.Web.Services;

public class GymApiClient
{
    private readonly HttpClient _http;

    public GymApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<GymClassSummary>> GetUpcomingClassesAsync()
    {
        return await _http.GetFromJsonAsync<List<GymClassSummary>>("api/classes") ?? new();
    }

    public async Task<List<BookingResponse>> GetClassBookingsAsync(int classId)
    {
        return await _http.GetFromJsonAsync<List<BookingResponse>>($"api/classes/{classId}/bookings") ?? new();
    }

    public async Task<MemberResponse?> GetMemberAsync(int memberId)
    {
        try { return await _http.GetFromJsonAsync<MemberResponse>($"api/members/{memberId}"); }
        catch { return null; }
    }

    public async Task<List<MemberBookingResponse>> GetMemberBookingsAsync(int memberId)
    {
        return await _http.GetFromJsonAsync<List<MemberBookingResponse>>($"api/members/{memberId}/bookings") ?? new();
    }

    public async Task<(BookingResponse? booking, string? error)> BookClassAsync(int memberId, int gymClassId)
    {
        var response = await _http.PostAsJsonAsync("api/bookings", new BookingRequest(memberId, gymClassId));
        if (response.IsSuccessStatusCode)
        {
            var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
            return (booking, null);
        }

        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (null, err?.Error ?? response.ReasonPhrase ?? "Unknown error");
    }

    public async Task<string?> CancelBookingAsync(int bookingId)
    {
        var response = await _http.DeleteAsync($"api/bookings/{bookingId}");
        if (response.IsSuccessStatusCode) return null;

        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return err?.Error ?? response.ReasonPhrase ?? "Unknown error";
    }

    private record ErrorResponse(string Error);
}
