using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task CreateVehicleAsync(Vehicle vehicle);
        // W przyszłości dodasz: UpdateVehicleAsync, DeleteVehicleAsync
    }
}