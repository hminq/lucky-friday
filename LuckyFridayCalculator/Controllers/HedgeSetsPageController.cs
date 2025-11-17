using Microsoft.AspNetCore.Mvc;

namespace LuckyFridayCalculator.Controllers;

public class HedgeSetsPageController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public HedgeSetsPageController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("/hedgesets")]
    public IActionResult GetHedgeSetsPage()
    {
        var htmlPath = Path.Combine(_environment.WebRootPath ?? string.Empty, "hedgesets.html");
        if (!System.IO.File.Exists(htmlPath))
        {
            return NotFound("hedgesets.html not found under wwwroot.");
        }

        var html = System.IO.File.ReadAllText(htmlPath);
        return Content(html, "text/html");
    }
}

