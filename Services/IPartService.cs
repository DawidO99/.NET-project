using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IPartService
    {
        Task<IEnumerable<Part>> GetAllPartsAsync();
        Task<Part?> GetPartByIdAsync(int id);
        Task CreatePartAsync(Part part);
        Task UpdatePartAsync(Part part);
        Task DeletePartAsync(int id);
    }
}