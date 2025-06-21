// Services/IVehicleService.cs
using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id); // Changed to nullable return
        Task CreateVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle); // Dodano
        Task DeleteVehicleAsync(int id); // Dodano
    }
}