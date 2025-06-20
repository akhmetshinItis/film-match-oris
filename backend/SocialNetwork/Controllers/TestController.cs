using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }
    [HttpGet("check-image")]
    public IActionResult CheckImage()
    {
        _logger.LogInformation("GET /Test/check-image");
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "category", "action.png");
        if (System.IO.File.Exists(imagePath))
        {
            _logger.LogInformation("Image exists: {Path}", imagePath);
            return Ok(new { exists = true, path = imagePath });
        }
        _logger.LogWarning("Image not found: {Path}", imagePath);
        return NotFound(new { exists = false, path = imagePath });
    }
} 