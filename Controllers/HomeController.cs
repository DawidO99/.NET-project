using CarWorkshopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarWorkshopManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogTrace("DIAGNOSTYKA NLOG: Strona g³ówna za³adowana. To jest wiadomoœæ TRACE."); // DODAJ T¥ LINIÊ
            _logger.LogInformation("DIAGNOSTYKA NLOG: Strona g³ówna za³adowana. To jest wiadomoœæ INFORMATION."); // DODAJ T¥ LINIÊ
            _logger.LogError(new Exception("To jest testowy wyj¹tek NLOG."), "DIAGNOSTYKA NLOG: Wyst¹pi³ testowy b³¹d na stronie g³ównej."); // DODAJ T¥ LINIÊ

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
