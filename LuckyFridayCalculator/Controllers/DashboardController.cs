using Microsoft.AspNetCore.Mvc;

namespace LuckyFridayCalculator.Controllers;

[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public DashboardController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("/dashboard")]
    public IActionResult GetDashboard()
    {
        var dashboardPath = Path.Combine(_environment.WebRootPath ?? string.Empty, "dashboard.html");
        if (!System.IO.File.Exists(dashboardPath))
        {
            return NotFound("dashboard.html not found under wwwroot.");
        }

        var now = DateTime.UtcNow.AddHours(7);
        var weekday = now.ToString("dddd");
        var date = now.ToString("MMMM dd, yyyy");

        var html = System.IO.File.ReadAllText(dashboardPath)
            .Replace("{{WEEKDAY}}", weekday)
            .Replace("{{DATE}}", date);

        return Content(html, "text/html");
    }
}

