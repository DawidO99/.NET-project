// Services/ICustomerService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManagementSystem.Models;
// Usunieto CarWorkshopManagementSystem.DTOs, bo serwis pracuje na encjach

namespace CarWorkshopManagementSystem.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer); // Nadal przyjmuje encję
        Task UpdateAsync(Customer customer); // Nadal przyjmuje encję
        Task DeleteAsync(int id);
    }
}