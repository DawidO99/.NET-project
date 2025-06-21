using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrzebne do SelectList
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService; // Potrzebujemy dostępu do klientów

        public VehiclesController(IVehicleService vehicleService, ICustomerService customerService)
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(vehicles);
        }

        // GET: Vehicles/Create
        public async Task<IActionResult> Create()
        {
            // Potrzebujemy listy klientów, aby wyświetlić ją w dropdownie
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Brand,Model,VIN,RegistrationNumber,Year,CustomerId")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.CreateVehicleAsync(vehicle);
                return RedirectToAction(nameof(Index));
            }

            // Jeśli model nie jest poprawny, musimy ponownie załadować listę klientów
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", vehicle.CustomerId);
            return View(vehicle);
        }
    }
}