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
        private readonly ILogger<VehicleService> _logger; // DODANO
        private readonly IWebHostEnvironment _webHostEnvironment; // DODANO

        public VehicleService(ApplicationDbContext context, ILogger<VehicleService> logger, IWebHostEnvironment webHostEnvironment) // ZMODYFIKOWANO
        {
            _context = context;
            _logger = logger; // Zainicjowanie loggera
            _webHostEnvironment = webHostEnvironment; // Zainicjowanie środowiska hostingu
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

        public async Task CreateVehicleAsync(Vehicle vehicle, IFormFile? imageFile) // ZMODYFIKOWANO
        {
            try
            {
                if (imageFile != null)
                {
                    vehicle.ImageUrl = await SaveImageAsync(imageFile);
                }

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
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
                throw;
            }
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle, IFormFile? newImageFile, bool removeCurrentImage) // ZMODYFIKOWANO
        {
            try
            {
                // Pobieramy istniejący pojazd, aby Entity Framework Core zaczął go śledzić
                var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
                if (existingVehicle == null)
                {
                    _logger.LogWarning("Próba aktualizacji nieistniejącego pojazdu o ID {VehicleId}.", vehicle.Id);
                    throw new KeyNotFoundException($"Pojazd o ID {vehicle.Id} nie został znaleziony.");
                }

                // Logika usuwania starego zdjęcia
                if (removeCurrentImage && !string.IsNullOrEmpty(existingVehicle.ImageUrl))
                {
                    DeleteImageFile(existingVehicle.ImageUrl);
                    existingVehicle.ImageUrl = null; // Ustaw URL na null, jeśli zdjęcie zostało usunięte
                }

                // Logika dodawania/aktualizacji nowego zdjęcia
                if (newImageFile != null)
                {
                    // Jeśli jest nowe zdjęcie, usuń stare (chyba że już zostało usunięte flagą removeCurrentImage)
                    if (!string.IsNullOrEmpty(existingVehicle.ImageUrl) && !removeCurrentImage)
                    {
                        DeleteImageFile(existingVehicle.ImageUrl);
                    }
                    existingVehicle.ImageUrl = await SaveImageAsync(newImageFile);
                }
                else if (string.IsNullOrEmpty(vehicle.ImageUrl) && !removeCurrentImage)
                {
                    // Jeśli ImageUrl z formularza jest pusty, a nie było flagi usuwania (np. użytkownik usunął URL ręcznie)
                    // i jest jakieś zdjęcie, to je usuń.
                    if (!string.IsNullOrEmpty(existingVehicle.ImageUrl))
                    {
                        DeleteImageFile(existingVehicle.ImageUrl);
                        existingVehicle.ImageUrl = null;
                    }
                }
                // Jeśli imageFile jest null, a removeCurrentImage false, i vehicle.ImageUrl jest pusty,
                // oznacza to, że użytkownik nie chce zmieniać zdjęcia i zostawił je puste,
                // więc istniejący URL powinien pozostać taki, jaki był przed edycją.
                // Logika powyżej to już obsłużyła lub zachowa istniejący URL, jeśli nie było nowego pliku ani flagi usuwania.


                // Aktualizuj pozostałe pola pojazdu ręcznie (aby nie nadpisywać ImageUrl, który już obsłużyliśmy)
                existingVehicle.Brand = vehicle.Brand;
                existingVehicle.Model = vehicle.Model;
                existingVehicle.VIN = vehicle.VIN;
                existingVehicle.RegistrationNumber = vehicle.RegistrationNumber;
                existingVehicle.Year = vehicle.Year;
                existingVehicle.CustomerId = vehicle.CustomerId;
                // existingVehicle.ImageUrl = vehicle.ImageUrl; // NIE AKTUALIZUJ TEGO TUTAJ, JUŻ TO OBSŁUŻYLIŚMY

                _context.Entry(existingVehicle).State = EntityState.Modified; // Oznacz jako zmodyfikowany
                await _context.SaveChangesAsync();
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
                    // Usuń zdjęcie pojazdu, jeśli istnieje
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
                _logger.LogError(ex, "Błąd podczas usuwania pojazdu o ID: {VehicleId}.", id);
                throw;
            }
        }

        // Metoda pomocnicza do zapisywania plików obrazów
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

            return "/uploads/" + uniqueFileName; // Zwróć ścieżkę względną do zapisania w bazie danych
        }

        // Metoda pomocnicza do usuwania plików obrazów
        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // Sprawdź, czy ścieżka zaczyna się od "/uploads/"
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
                        // Możesz zdecydować, czy rzucić wyjątek, czy tylko zalogować
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