using FilmMatch.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmMatch.Controllers
{
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }
        [HttpGet("/notification")]
        public async Task<IActionResult> SendNotification([FromQuery] Guid userId)
        {
            _logger.LogInformation("GET /notification: userId={UserId}", userId);
            await _notificationService.SendNotificationAsync(userId, "test");
            _logger.LogInformation("Notification sent to {UserId}", userId);
            return Ok();
        }
    }
}