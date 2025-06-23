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
            _logger.LogTrace("DIAGNOSTYKA NLOG: Strona g��wna za�adowana. To jest wiadomo�� TRACE."); // DODAJ T� LINI�
            _logger.LogInformation("DIAGNOSTYKA NLOG: Strona g��wna za�adowana. To jest wiadomo�� INFORMATION."); // DODAJ T� LINI�
            _logger.LogError(new Exception("To jest testowy wyj�tek NLOG."), "DIAGNOSTYKA NLOG: Wyst�pi� testowy b��d na stronie g��wnej."); // DODAJ T� LINI�

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
