// Controllers/ServiceOrdersController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Potrzebne do UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrzebne do SelectList i SelectListItem
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Dodaj to, jeśli jeszcze nie masz

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista,Mechanik")]
    public class ServiceOrdersController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<AppUser> _userManager; // Serwis do zarządzania użytkownikami
        private readonly IPartService _partService; // DODANE: Serwis do zarządzania częściami

        // ZMODYFIKOWANY KONSTRUKTOR: Dodano IPartService
        public ServiceOrdersController(IServiceOrderService orderService, IVehicleService vehicleService, UserManager<AppUser> userManager, IPartService partService)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _userManager = userManager;
            _partService = partService; // Zainicjowanie serwisu części
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,Description,AssignedMechanicId")] ServiceOrder serviceOrder)
        {
            ModelState.Remove("Vehicle");

            if (ModelState.IsValid)
            {
                await _orderService.CreateOrderAsync(serviceOrder);
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(serviceOrder);
            return View(serviceOrder);
        }

        // GET: ServiceOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Upewnij się, że GetOrderByIdAsync wczytuje wszystkie potrzebne relacje (Tasks, UsedParts, Parts, Comments, Author, Vehicle, Customer, Mechanic)
            // Jeśli nadal masz problemy z brakującymi danymi po tej zmianie, będziemy musieli sprawdzić ServiceOrderService.cs
            var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);

            if (serviceOrder == null)
            {
                return NotFound();
            }

            // DODANY KOD: Ładowanie dostępnych części dla dropdownów w formularzu dodawania czynności
            var parts = await _partService.GetAllPartsAsync();
            ViewBag.Parts = parts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.UnitPrice:C})" // Formatowanie nazwy i ceny dla widoku
            }).ToList();


            return View(serviceOrder);
        }

        // Prywatna metoda pomocnicza do ładowania danych dla dropdownów
        private async Task PopulateDropdowns(ServiceOrder? model = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");

            // Tworzymy listę pojazdów w formacie "Marka Model (Rejestracja)"
            var vehicleList = vehicles.Select(v => new
            {
                v.Id,
                DisplayText = $"{v.Brand} {v.Model} ({v.RegistrationNumber})"
            });

            ViewBag.Vehicles = new SelectList(vehicleList, "Id", "DisplayText", model?.VehicleId);
            ViewBag.Mechanics = new SelectList(mechanics, "Id", "UserName", model?.AssignedMechanicId);
        }
    }
}