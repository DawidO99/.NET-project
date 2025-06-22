// Controllers/VehiclesController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Collections.Generic;
using System; // Potrzebne dla KeyNotFoundException, Exception
using Microsoft.Extensions.Logging; // DODANO: Dla ILogger
using Microsoft.AspNetCore.Http; // DODANO: Dla IFormFile
using Microsoft.EntityFrameworkCore; // DODANO: Dla DbUpdateConcurrencyException
using System.Linq; // DODANO: Dla SelectMany


namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<VehiclesController> _logger; // DODANO

        public VehiclesController(IVehicleService vehicleService, ICustomerService customerService, ILogger<VehiclesController> logger) // ZMODYFIKOWANO
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
            _logger = logger; // Zainicjowanie loggera
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                _logger.LogInformation("Pobrano {Count} pojazdów dla strony głównej.", vehicles.Count());
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich pojazdów.");
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania listy pojazdów.";
                return View(new List<Vehicle>()); // Zwróć pustą listę w razie błędu
            }
        }

        // GET: Vehicles/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    _logger.LogWarning("Pojazd o ID {VehicleId} nie znaleziono podczas próby wyświetlenia szczegółów.", id);
                    return NotFound();
                }
                _logger.LogInformation("Pobrano szczegóły pojazdu ID {VehicleId}.", id);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania szczegółów pojazdu ID {VehicleId}.", id);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania szczegółów pojazdu.";
                return RedirectToAction(nameof(Index));
            }
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
        public async Task<IActionResult> Create([Bind("Brand,Model,VIN,RegistrationNumber,Year,CustomerId")] Vehicle vehicle, [FromForm] IFormFile? imageFile)
        {
            // ModelState.Remove("Customer"); // W modelu Vehicle.cs dodano [ValidateNever] dla Customer, więc ta linia powinna być zbędna.

            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.CreateVehicleAsync(vehicle, imageFile);
                    _logger.LogInformation("Utworzono nowy pojazd: {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
                    TempData["SuccessMessage"] = "Pojazd został pomyślnie dodany.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas tworzenia pojazdu: {Brand} {Model}.", vehicle.Brand, vehicle.Model);
                    TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania pojazdu. Spróbuj ponownie.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas tworzenia pojazdu: {Errors}", string.Join("; ", errors));
                TempData["ErrorMessage"] = "Nie udało się dodać pojazdu. Sprawdź poprawność danych.";
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
                _logger.LogWarning("Błąd: Brak ID pojazdu w żądaniu edycji.");
                return NotFound();
            }

            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
                if (vehicle == null)
                {
                    _logger.LogWarning("Pojazd o ID {VehicleId} nie znaleziono podczas próby edycji.", id.Value);
                    return NotFound();
                }

                var customers = await _customerService.GetAllAsync();
                ViewBag.Customers = new SelectList(customers, "Id", "FullName", vehicle.CustomerId);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania strony edycji pojazdu ID {VehicleId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania danych pojazdu do edycji.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicles/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,VIN,RegistrationNumber,Year,CustomerId")] Vehicle vehicle, [FromForm] IFormFile? newImageFile, [FromForm] bool removeCurrentImage)
        {
            if (id != vehicle.Id)
            {
                _logger.LogWarning("Błąd: Niezgodność ID pojazdu w żądaniu edycji. URL ID: {UrlId}, Model ID: {ModelId}.", id, vehicle.Id);
                return NotFound();
            }

            // ModelState.Remove("Customer"); // W modelu Vehicle.cs dodano [ValidateNever] dla Customer, więc ta linia powinna być zbędna.

            // Zadeklaruj 'customers' raz na początku, aby uniknąć CS0136
            List<Customer> customers;
            Vehicle modelToReturn = vehicle; // Użyj tej zmiennej, aby zwrócić odpowiedni model do widoku

            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.UpdateVehicleAsync(vehicle, newImageFile, removeCurrentImage);
                    _logger.LogInformation("Zaktualizowano pojazd: {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
                    TempData["SuccessMessage"] = "Pojazd został pomyślnie zaktualizowany.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Próba aktualizacji nieistniejącego pojazdu o ID {VehicleId}.", id);
                    TempData["ErrorMessage"] = $"Błąd: Pojazd o ID {id} nie został znaleziony.";
                    return NotFound();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Konflikt współbieżności podczas edycji pojazdu ID {VehicleId}.", id);
                    TempData["ErrorMessage"] = "Błąd: Dane pojazdu zostały zmienione przez innego użytkownika. Spróbuj ponownie.";

                    // W przypadku konfliktu, załaduj świeże dane z bazy i zwróć je do widoku,
                    // aby użytkownik mógł zobaczyć aktualny stan i podjąć decyzję.
                    var freshVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                    modelToReturn = freshVehicle ?? vehicle; // Jeśli freshVehicle to null, użyj pierwotnego vehicle

                    // Załaduj klientów dla dropdowna w widoku
                    customers = await _customerService.GetAllAsync();
                    ViewBag.Customers = new SelectList(customers, "Id", "FullName", modelToReturn?.CustomerId);
                    return View(modelToReturn);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji pojazdu: {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
                    TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas aktualizacji pojazdu. Spróbuj ponownie.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas edycji pojazdu: {Errors}", string.Join("; ", errors));
                TempData["ErrorMessage"] = "Nie udało się zaktualizować pojazdu. Sprawdź poprawność danych.";
            }

            // Ta linia zostanie wykonana, jeśli ModelState.IsValid jest false lub wystąpi inny błąd (poza DbUpdateConcurrencyException)
            customers = await _customerService.GetAllAsync(); // Przypisanie, nie deklaracja
            ViewBag.Customers = new SelectList(customers, "Id", "FullName", modelToReturn.CustomerId);
            return View(modelToReturn); // Zwróć model, który był przekazany lub zaktualizowany w przypadku błędu walidacji/ogólnego
        }

        // GET: Vehicles/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Błąd: Brak ID pojazdu w żądaniu usunięcia.");
                return NotFound();
            }

            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
                if (vehicle == null)
                {
                    _logger.LogWarning("Pojazd o ID {VehicleId} nie znaleziono podczas próby usunięcia.", id.Value);
                    return NotFound();
                }
                _logger.LogInformation("Pobrano pojazd ID {VehicleId} do usunięcia.", id.Value);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania strony usuwania pojazdu ID {VehicleId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania danych pojazdu do usunięcia.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicles/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _vehicleService.DeleteVehicleAsync(id);
                _logger.LogInformation("Usunięto pojazd ID {VehicleId}.", id);
                TempData["SuccessMessage"] = $"Pojazd został pomyślnie usunięty.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Próba usunięcia nieistniejącego pojazdu o ID {VehicleId}.", id);
                TempData["ErrorMessage"] = $"Błąd: Pojazd o ID {id} nie został znaleziony.";
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania pojazdu o ID {VehicleId}.", id);
                TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas usuwania pojazdu. Spróbuj ponownie.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}