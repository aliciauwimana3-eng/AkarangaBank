using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AkarangaBank.Web.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userId, out var id)) await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{id}");
        await base.OnConnectedAsync();
    }
}
