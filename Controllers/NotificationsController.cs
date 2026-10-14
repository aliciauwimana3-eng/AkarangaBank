using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;

namespace AkarangaBank.Web.Controllers;

public sealed class NotificationsController : Controller
{
    private readonly IUserNotificationService _notifications;
    private readonly ICurrentUserService _currentUser;
    private readonly AkarangaBankContext _db;

    public NotificationsController(IUserNotificationService notifications, ICurrentUserService currentUser, AkarangaBankContext db)
    {
        _notifications = notifications;
        _currentUser = currentUser;
        _db = db;
    }

    [HttpPost("Notifications/PushToken"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PushToken([FromForm] string token, CancellationToken cancellationToken)
    {
        var value = (token ?? string.Empty).Trim();
        if (value.Length is < 20 or > 512) return BadRequest("Jeton push invalide.");
        var userId = await _currentUser.GetUserIdAsync();
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId && item.EstActif, cancellationToken);
        if (user is null) return Unauthorized();
        user.PushToken = value;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetUserIdAsync();
        return View(await _notifications.GetForUserAsync(userId, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetUserIdAsync();
        await _notifications.MarkReadAsync(id, userId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
