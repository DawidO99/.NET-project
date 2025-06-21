// Controllers/VehiclesController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Collections.Generic; // Potrzebne dla IEnumerable<Vehicle>
using System; // Potrzebne dla KeyNotFoundException

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;

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

        // GET: Vehicles/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // GET: Vehicles/Create
        public async Task<IActionResult> Create(int? customerId)
        {
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", customerId);

            if (customerId.HasValue)
            {
                var vehicle = new Vehicle { CustomerId = customerId.Value };
                return View(vehicle);
            }

            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Brand,Model,VIN,RegistrationNumber,Year,ImageUrl,CustomerId")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.CreateVehicleAsync(vehicle);
                return RedirectToAction(nameof(Index));
            }

            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", vehicle.CustomerId);
            return View(vehicle);
        }

        // GET: Vehicles/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", vehicle.CustomerId);
            return View(vehicle);
        }

        // POST: Vehicles/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,VIN,RegistrationNumber,Year,ImageUrl,CustomerId")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.UpdateVehicleAsync(vehicle);
                }
                catch (KeyNotFoundException) // Assuming your service throws this if not found
                {
                    return NotFound();
                }
                catch (Exception) // Catch other potential exceptions during save
                {
                    // Log the exception (recommended)
                    // Add ModelState error for user feedback
                    ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji pojazdu. Spróbuj ponownie.");
                }
            }

            // If ModelState is not valid or an exception occurred, reload customers and return view
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", vehicle.CustomerId);
            return View(vehicle);
        }

        // GET: Vehicles/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}