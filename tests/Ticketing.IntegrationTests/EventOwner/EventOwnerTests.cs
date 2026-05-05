using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Ticketing.Domain.Enums;
using Ticketing.IntegrationTests.Common;

namespace Ticketing.IntegrationTests.EventOwner;

public class EventOwnerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestAuthHelper _authHelper;

    public EventOwnerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authHelper = new TestAuthHelper(factory, _client);
    }

    [Fact]
    public async Task Create_Event_Should_Return_Success_When_Data_Is_Valid()
    {
        var eventOwner = await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var request = new
        {
            name = "Test Event",
            description = "Test Description",
            bookingMode = EventBookingMode.Seats
        };

        var response = await _client.PostAsJsonAsync("/api/event-owner/events", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CReate_Event_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new
        {
            name = "Test Event",
            description = "Test Description",
            bookingMode = EventBookingMode.Seats
        };

        var response = await _client.PostAsJsonAsync("/api/event-owner/events", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_My_Events_Should_Return_Success_When_User_Is_Authenticated()
    {
        var eventOwner = await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var response = await _client.GetAsync("/api/event-owner/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Session_Should_Return_Success_When_Data_Is_Valid()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var createEventRequest = new
        {
            name = "Session parent event",
            description = "For session test",
            bookingMode = EventBookingMode.Seats
        };

        var createEventResponse =
            await _client.PostAsJsonAsync("/api/event-owner/events", createEventRequest);
        createEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var eventId = await TestAuthHelper.ExtractIdFromResponseAsync(createEventResponse);

        var sessionRequest = new
        {
            startTime = DateTime.UtcNow.AddHours(1),
            endTime = DateTime.UtcNow.AddHours(2),
            location = "Test Location"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/events/{eventId}/sessions",
            sessionRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Session_Should_Return_BadRequest_When_StartTime_Is_After_EndTime()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var createEventRequest = new
        {
            name = "Session parent event",
            description = "For session test",
            bookingMode = EventBookingMode.Seats
        };

        var createEventResponse =
            await _client.PostAsJsonAsync("/api/event-owner/events", createEventRequest);
        createEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var eventId = await TestAuthHelper.ExtractIdFromResponseAsync(createEventResponse);

        var sessionRequest = new
        {
            startTime = DateTime.UtcNow.AddHours(2),
            endTime = DateTime.UtcNow.AddHours(1),
            location = "Test Location"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/events/{eventId}/sessions",
            sessionRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSession_Should_Return_Error_When_Event_Does_Not_Belong_To_EventOwner()
    {
        // Owner A login
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync(
            email: $"owner-a-{Guid.NewGuid()}@test.com");

        // Owner A creates event
        var createEventRequest = new
        {
            name = "Owner A Event",
            description = "Test description",
            bookingMode = EventBookingMode.Seats
        };

        var createEventResponse = await _client.PostAsJsonAsync(
            "/api/event-owner/events",
            createEventRequest);

        createEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var eventId = await TestAuthHelper.ExtractIdFromResponseAsync(createEventResponse);

        // Clear Owner A token
        _client.DefaultRequestHeaders.Authorization = null;

        // Owner B login
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync(
            email: $"owner-b-{Guid.NewGuid()}@test.com");

        // Owner B tries to add session to Owner A event
        var addSessionRequest = new
        {
            startTime = DateTime.UtcNow.AddDays(1),
            endTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            location = "Cairo"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/events/{eventId}/sessions",
            addSessionRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static StringContent EmptyJsonBody() =>
        new StringContent("{}", Encoding.UTF8, "application/json");

    private async Task<(int EventId, int SessionId)> CreateEventWithOneSessionAsync(EventBookingMode bookingMode)
    {
        var createEventResponse = await _client.PostAsJsonAsync(
            "/api/event-owner/events",
            new
            {
                name = $"Evt-{Guid.NewGuid():N}",
                description = "Test",
                bookingMode
            });
        createEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventId = await TestAuthHelper.ExtractIdFromResponseAsync(createEventResponse);
        var sessionResponse = await _client.PostAsJsonAsync(
            $"/api/event-owner/events/{eventId}/sessions",
            new
            {
                startTime = DateTime.UtcNow.AddDays(2),
                endTime = DateTime.UtcNow.AddDays(2).AddHours(3),
                location = "Main hall"
            });
        sessionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionId = await _authHelper.GetLatestSessionIdForEventAsync(eventId);
        return (eventId, sessionId);
    }

    [Fact]
    public async Task AddSeats_Should_Return_Success_When_Event_BookingMode_Is_Seats()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var (_, sessionId) = await CreateEventWithOneSessionAsync(EventBookingMode.Seats);

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/seats",
            new
            {
                seatNumber = "A1",
                type = "Standard",
                price = 25m,
                eventSessionId = sessionId
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddTicketTypes_Should_Return_Success_When_Event_BookingMode_Is_Tickets()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var (_, sessionId) = await CreateEventWithOneSessionAsync(EventBookingMode.Tickets);

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/ticket-types",
            new
            {
                name = "General",
                price = 40m,
                totalQuantity = 100,
                eventSessionId = sessionId
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddSeats_Should_Return_BadRequest_When_Event_BookingMode_Is_Tickets()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var (_, sessionId) = await CreateEventWithOneSessionAsync(EventBookingMode.Tickets);

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/seats",
            new
            {
                seatNumber = "A1",
                type = "Standard",
                price = 25m,
                eventSessionId = sessionId
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddTicketTypes_Should_Return_BadRequest_When_Event_BookingMode_Is_Seats()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var (_, sessionId) = await CreateEventWithOneSessionAsync(EventBookingMode.Seats);

        var response = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/ticket-types",
            new
            {
                name = "General",
                price = 40m,
                totalQuantity = 50,
                eventSessionId = sessionId
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PublishEvent_Should_Return_BadRequest_When_Event_Has_No_Sessions()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var createEventResponse = await _client.PostAsJsonAsync(
            "/api/event-owner/events",
            new
            {
                name = "No sessions event",
                description = "Test",
                bookingMode = EventBookingMode.Seats
            });
        createEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventId = await TestAuthHelper.ExtractIdFromResponseAsync(createEventResponse);

        var response = await _client.PostAsync(
            $"/api/event-owner/events/{eventId}/publish",
            EmptyJsonBody());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PublishEvent_Should_Return_Success_When_Event_Is_Ready()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        var (eventId, sessionId) = await CreateEventWithOneSessionAsync(EventBookingMode.Seats);

        var seatResponse = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/seats",
            new
            {
                seatNumber = "B2",
                type = "VIP",
                price = 99m,
                eventSessionId = sessionId
            });
        seatResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PostAsync(
            $"/api/event-owner/events/{eventId}/publish",
            EmptyJsonBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dashboard_Should_Return_Correct_Basic_Counts_For_EventOwner()
    {
        await _authHelper.CreateConfirmedEventOwnerAndLoginAsync();

        for (var i = 0; i < 2; i++)
        {
            var draft = await _client.PostAsJsonAsync(
                "/api/event-owner/events",
                new
                {
                    name = $"Draft-{i}-{Guid.NewGuid():N}",
                    description = "Draft only",
                    bookingMode = EventBookingMode.Seats
                });
            draft.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var (publishEventId, sessionId) =
            await CreateEventWithOneSessionAsync(EventBookingMode.Seats);

        var seatResponse = await _client.PostAsJsonAsync(
            $"/api/event-owner/sessions/{sessionId}/seats",
            new
            {
                seatNumber = "C3",
                type = "Standard",
                price = 15m,
                eventSessionId = sessionId
            });
        seatResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publishResponse = await _client.PostAsync(
            $"/api/event-owner/events/{publishEventId}/publish",
            EmptyJsonBody());
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboardResponse = await _client.GetAsync("/api/event-owner/dashboard");
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await dashboardResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("totalEvents").GetInt32().Should().Be(3);
        data.GetProperty("draftEvents").GetInt32().Should().Be(2);
        data.GetProperty("publishedEvents").GetInt32().Should().Be(1);
        data.GetProperty("totalSessions").GetInt32().Should().Be(1);
        data.GetProperty("totalCapacity").GetInt32().Should().Be(1);
        data.GetProperty("unpublishedEvents").GetInt32().Should().Be(2);
    }
}
