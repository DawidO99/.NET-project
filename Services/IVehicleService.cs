// Services/IVehicleService.cs
using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // DODANO: Potrzebne dla IFormFile

namespace CarWorkshopManagementSystem.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task CreateVehicleAsync(Vehicle vehicle, IFormFile? imageFile); // ZMIENIONO: Dodano IFormFile?
        Task UpdateVehicleAsync(Vehicle vehicle, IFormFile? newImageFile, bool removeCurrentImage); // ZMIENIONO: Dodano IFormFile? i bool
        Task DeleteVehicleAsync(int id);
    }
}