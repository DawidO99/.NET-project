// Controllers/ReportsController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Models.ViewModels;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Dodano dla SelectList
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista")] // Tylko Admin i Recepcjonista mogą generować raporty
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IServiceOrderService _serviceOrderService; // Potrzebne do pobierania danych do ViewModeli dla raportów
        private readonly ICustomerService _customerService; // Potrzebne do listy klientów
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, IServiceOrderService serviceOrderService,
                                 ICustomerService customerService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _serviceOrderService = serviceOrderService;
            _customerService = customerService;
            _logger = logger;
        }

        // GET: /Reports
        public IActionResult Index()
        {
            _logger.LogInformation("Użytkownik wszedł na stronę główną raportów.");
            return View();
        }

        // GET: /Reports/CustomerReport
        public async Task<IActionResult> CustomerReport()
        {
            await PopulateCustomersDropdown();
            _logger.LogInformation("Użytkownik przygotowuje raport klienta.");
            return View();
        }

        // POST: /Reports/GenerateCustomerReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCustomerReport(int customerId)
        {
            if (customerId <= 0)
            {
                TempData["ErrorMessage"] = "Wybierz klienta, dla którego chcesz wygenerować raport.";
                await PopulateCustomersDropdown();
                return View("CustomerReport"); // Wracamy do widoku formularza
            }

            try
            {
                var reportData = await _serviceOrderService.GetCustomerRepairReportDataAsync(customerId);

                if (reportData == null)
                {
                    _logger.LogWarning("Nie znaleziono danych dla raportu klienta o ID {CustomerId}.", customerId);
                    TempData["ErrorMessage"] = $"Nie znaleziono danych napraw dla klienta o ID {customerId}.";
                    await PopulateCustomersDropdown();
                    return View("CustomerReport");
                }

                // Generowanie PDF
                var pdfBytes = await _reportService.GenerateCustomerRepairReportPdfAsync(reportData);

                _logger.LogInformation("Wygenerowano raport PDF dla klienta ID {CustomerId}.", customerId);
                return File(pdfBytes, "application/pdf", $"Raport_Napraw_Klienta_{reportData.CustomerFullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania raportu klienta o ID {CustomerId}.", customerId);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas generowania raportu klienta. Spróbuj ponownie.";
                await PopulateCustomersDropdown();
                return View("CustomerReport");
            }
        }

        // GET: /Reports/MonthlyReport
        public IActionResult MonthlyReport()
        {
            // Przygotowanie danych do wyboru roku i miesiąca
            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 5, 10) // Ostatnie 5 lat i kolejne 5 lat
                                      .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() })
                                      .ToList();
            ViewBag.Months = Enumerable.Range(1, 12)
                                       .Select(m => new SelectListItem { Value = m.ToString(), Text = new DateTime(2000, m, 1).ToString("MMMM") })
                                       .ToList();
            _logger.LogInformation("Użytkownik przygotowuje raport miesięczny.");
            return View();
        }

        // POST: /Reports/GenerateMonthlyReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMonthlyReport(int year, int month)
        {
            if (year <= 0 || month <= 0 || month > 12)
            {
                TempData["ErrorMessage"] = "Wybierz poprawny rok i miesiąc.";
                // Ponownie przygotuj ViewBag
                ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 5, 10).Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() }).ToList();
                ViewBag.Months = Enumerable.Range(1, 12).Select(m => new SelectListItem { Value = m.ToString(), Text = new DateTime(2000, m, 1).ToString("MMMM") }).ToList();
                return View("MonthlyReport");
            }

            try
            {
                var reportData = await _serviceOrderService.GetMonthlyRepairReportDataAsync(year, month);

                if (!reportData.Any())
                {
                    _logger.LogWarning("Brak danych dla miesięcznego raportu za {Month}/{Year}.", month, year);
                    TempData["ErrorMessage"] = $"Brak danych napraw za {new DateTime(year, month, 1).ToString("MMMM yyyy")}.";
                    // Ponownie przygotuj ViewBag
                    ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 5, 10).Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() }).ToList();
                    ViewBag.Months = Enumerable.Range(1, 12).Select(m => new SelectListItem { Value = m.ToString(), Text = new DateTime(2000, m, 1).ToString("MMMM") }).ToList();
                    return View("MonthlyReport");
                }

                // Generowanie PDF
                var pdfBytes = await _reportService.GenerateMonthlyRepairReportPdfAsync(reportData, year, month);

                _logger.LogInformation("Wygenerowano miesięczny raport PDF za {Month}/{Year}.", month, year);
                return File(pdfBytes, "application/pdf", $"Raport_Miesieczny_{year}_{month:00}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania miesięcznego raportu PDF za {Month}/{Year}.", month, year);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas generowania raportu miesięcznego. Spróbuj ponownie.";
                // Ponownie przygotuj ViewBag
                ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 5, 10).Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() }).ToList();
                ViewBag.Months = Enumerable.Range(1, 12).Select(m => new SelectListItem { Value = m.ToString(), Text = new DateTime(2000, m, 1).ToString("MMMM") }).ToList();
                return View("MonthlyReport");
            }
        }

        private async Task PopulateCustomersDropdown(int? selectedCustomerId = null)
        {
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", selectedCustomerId);
        }
    }
}