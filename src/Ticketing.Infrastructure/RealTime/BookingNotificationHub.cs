using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ticketing.Infrastructure.RealTime;

[Authorize]
public class BookingNotificationHub : Hub
{
    public const string Path = "/hubs/booking";

    public static string SessionGroupName(int sessionId) => $"session-{sessionId}";

    public async Task JoinSession(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroupName(sessionId));
    }

    public async Task LeaveSession(int sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SessionGroupName(sessionId));
    }
}
