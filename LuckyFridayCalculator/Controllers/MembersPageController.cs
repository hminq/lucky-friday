using Microsoft.AspNetCore.Mvc;

namespace LuckyFridayCalculator.Controllers;

public class MembersPageController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public MembersPageController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("/members")]
    public IActionResult GetMembersPage()
    {
        var htmlPath = Path.Combine(_environment.WebRootPath ?? string.Empty, "members.html");
        if (!System.IO.File.Exists(htmlPath))
        {
            return NotFound("members.html not found under wwwroot.");
        }

        var html = System.IO.File.ReadAllText(htmlPath);
        return Content(html, "text/html");
    }
}

