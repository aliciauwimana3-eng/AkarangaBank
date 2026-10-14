using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.ViewComponents;

public sealed class NotificationBellViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserNotificationService _notifications;

    public NotificationBellViewComponent(ICurrentUserService currentUser, IUserNotificationService notifications)
    {
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        var userId = await _currentUser.GetUserIdAsync();
        var unreadCount = await _notifications.GetUnreadCountAsync(userId);
        return View(unreadCount);
    }
}
