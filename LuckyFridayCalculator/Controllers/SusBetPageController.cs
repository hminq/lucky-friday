using Microsoft.AspNetCore.Mvc;

namespace LuckyFridayCalculator.Controllers
{
    [Route("susbet")]
    public class SusBetPageController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return File("~/susbet.html", "text/html");
        }
    }
}
