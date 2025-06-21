// Services/VehicleService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // For KeyNotFoundException

namespace CarWorkshopManagementSystem.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            // Używamy Include, aby załadować dane powiązanego Klienta
            return await _context.Vehicles.Include(v => v.Customer).ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id) // Zmieniono na nullable return
        {
            // WAŻNE: Dołączamy zarówno Customer, jak i Orders (zleceń serwisowych)
            // Aby uniknąć błędów 'Invalid column name' dla Order.CreationDate, upewnij się, że baza danych jest zaktualizowana migracją
            return await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.Orders)
                    .ThenInclude(o => o.AssignedMechanic) // Dołączamy mechanika do zlecenia serwisowego
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task CreateVehicleAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        // Dodano metody Update i Delete
        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            // Sprawdź, czy pojazd istnieje przed aktualizacją
            var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existingVehicle == null)
            {
                throw new KeyNotFoundException($"Pojazd o ID {vehicle.Id} nie został znaleziony.");
            }

            // Aktualizuj tylko zmienione pola
            _context.Entry(existingVehicle).CurrentValues.SetValues(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }
    }
}