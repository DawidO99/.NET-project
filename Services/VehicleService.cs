// Services/VehicleService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // For KeyNotFoundException, Exception
using Microsoft.AspNetCore.Http; // DODANO: Dla IFormFile
using System.IO; // DODANO: Dla Path, FileStream
using Microsoft.Extensions.Logging; // DODANO: Dla ILogger
using Microsoft.AspNetCore.Hosting; // DODANO: Dla IWebHostEnvironment

namespace CarWorkshopManagementSystem.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VehicleService(ApplicationDbContext context, ILogger<VehicleService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            try
            {
                var vehicles = await _context.Vehicles.Include(v => v.Customer).ToListAsync();
                _logger.LogInformation("Pobrano {Count} pojazdów.", vehicles.Count);
                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich pojazdów.");
                throw;
            }
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Customer)
                    .Include(v => v.Orders)
                        .ThenInclude(o => o.AssignedMechanic)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Pojazd o ID {VehicleId} nie znaleziono.", id);
                }
                else
                {
                    _logger.LogInformation("Pobrano pojazd o ID {VehicleId}.", id);
                }
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania pojazdu o ID {VehicleId}.", id);
                throw;
            }
        }

        public async Task CreateVehicleAsync(Vehicle vehicle, IFormFile? imageFile)
        {
            try
            {
                // Krok 1: Sprawdź i zapisz plik, jeśli istnieje
                if (imageFile != null)
                {
                    // Ustaw ImageUrl PRZED dodaniem do kontekstu, jeśli ID jest generowane przez bazę,
                    // to przy pierwszym SaveChanges() ImageUrl zostanie zapisany.
                    vehicle.ImageUrl = await SaveImageAsync(imageFile);
                }

                // Krok 2: Dodaj pojazd do kontekstu (teraz już z ImageUrl, jeśli było)
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync(); // Zapisz wszystko w jednej transakcji

                _logger.LogInformation("Dodano nowy pojazd {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia pojazdu {Brand} {Model}.", vehicle.Brand, vehicle.Model);
                // W przypadku błędu zapisu do DB, usuń zapisany plik, jeśli istnieje
                if (!string.IsNullOrEmpty(vehicle.ImageUrl))
                {
                    DeleteImageFile(vehicle.ImageUrl);
                }
                throw; // Przekaż wyjątek dalej, aby kontroler mógł go obsłużyć
            }
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle, IFormFile? newImageFile, bool removeCurrentImage)
        {
            try
            {
                // Pobieramy istniejący pojazd. FindAsync() śledzi ten obiekt.
                var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
                if (existingVehicle == null)
                {
                    _logger.LogWarning("Próba aktualizacji nieistniejącego pojazdu o ID {VehicleId}.", vehicle.Id);
                    throw new KeyNotFoundException($"Pojazd o ID {vehicle.Id} nie został znaleziony.");
                }

                string? currentDbImageUrl = existingVehicle.ImageUrl; // Pobierz aktualny URL z DB

                // Logika usuwania starego zdjęcia
                if (removeCurrentImage)
                {
                    if (!string.IsNullOrEmpty(currentDbImageUrl))
                    {
                        DeleteImageFile(currentDbImageUrl);
                    }
                    existingVehicle.ImageUrl = null; // Ustaw na null na śledzonym obiekcie
                    _logger.LogInformation("Usunięto istniejące zdjęcie dla pojazdu ID {VehicleId}.", vehicle.Id);
                }

                // Logika dodawania/aktualizacji nowego zdjęcia
                if (newImageFile != null)
                {
                    // Jeśli jest nowe zdjęcie, usuń stare, jeśli istniało i nie zostało już usunięte
                    if (!string.IsNullOrEmpty(currentDbImageUrl) && !removeCurrentImage)
                    {
                        DeleteImageFile(currentDbImageUrl);
                        _logger.LogInformation("Zastąpiono stare zdjęcie dla pojazdu ID {VehicleId}.", vehicle.Id);
                    }
                    existingVehicle.ImageUrl = await SaveImageAsync(newImageFile); // Zapisz nowy plik i ustaw URL na śledzonym obiekcie
                    _logger.LogInformation("Zapisano nowe zdjęcie dla pojazdu ID {VehicleId}: {ImageUrl}.", vehicle.Id, existingVehicle.ImageUrl);
                }
                // Jeśli newImageFile jest null i removeCurrentImage jest false,
                // istniejący existingVehicle.ImageUrl pozostaje bez zmian (czyli tak jak currentDbImageUrl).


                // Kopiowanie pozostałych właściwości z obiektu `vehicle` (z formularza)
                // do śledzonego obiektu `existingVehicle`.
                // EF Core automatycznie wykryje zmiany we właściwościach `existingVehicle`.
                existingVehicle.Brand = vehicle.Brand;
                existingVehicle.Model = vehicle.Model;
                existingVehicle.VIN = vehicle.VIN;
                existingVehicle.RegistrationNumber = vehicle.RegistrationNumber;
                existingVehicle.Year = vehicle.Year;
                existingVehicle.CustomerId = vehicle.CustomerId;

                // Nie ma potrzeby jawnie ustawiać .State = Modified; dla całego obiektu,
                // ani .Property(v => v.ImageUrl).IsModified = true;
                // ponieważ existingVehicle jest śledzony przez FindAsync, a jego properties zostały zmienione.

                await _context.SaveChangesAsync(); // Zapisz wszystkie wykryte zmiany
                _logger.LogInformation("Zaktualizowano pojazd {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Konflikt współbieżności podczas aktualizacji pojazdu {VehicleId}.", vehicle.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji pojazdu {Brand} {Model} (ID: {VehicleId}).", vehicle.Brand, vehicle.Model, vehicle.Id);
                throw;
            }
        }

        public async Task DeleteVehicleAsync(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle != null)
                {
                    if (!string.IsNullOrEmpty(vehicle.ImageUrl))
                    {
                        DeleteImageFile(vehicle.ImageUrl);
                    }

                    _context.Vehicles.Remove(vehicle);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Usunięto pojazd o ID: {VehicleId} ({Brand} {Model}).", id, vehicle.Brand, vehicle.Model);
                }
                else
                {
                    _logger.LogWarning("Próba usunięcia nieistniejącego pojazdu o ID: {VehicleId}.", id);
                    throw new KeyNotFoundException($"Pojazd o ID {id} nie został znaleziony.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania pojazdu o ID {VehicleId}.", id);
                throw;
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            if (imageUrl.StartsWith("/uploads/"))
            {
                var fileName = imageUrl.Substring("/uploads/".Length);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("Usunięto plik obrazu: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Błąd podczas usuwania pliku obrazu: {FilePath}", filePath);
                    }
                }
                else
                {
                    _logger.LogWarning("Próba usunięcia nieistniejącego pliku obrazu: {FilePath}", filePath);
                }
            }
            else
            {
                _logger.LogWarning("Nieprawidłowy format ImageUrl do usunięcia: {ImageUrl}", imageUrl);
            }
        }
    }
}