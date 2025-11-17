using Microsoft.AspNetCore.Mvc;

namespace LuckyFridayCalculator.Controllers;

public class FridaysPageController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public FridaysPageController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("/fridays")]
    public IActionResult GetFridaysPage()
    {
        var htmlPath = Path.Combine(_environment.WebRootPath ?? string.Empty, "fridays.html");
        if (!System.IO.File.Exists(htmlPath))
        {
            return NotFound("fridays.html not found under wwwroot.");
        }

        var html = System.IO.File.ReadAllText(htmlPath);
        return Content(html, "text/html");
    }
}

