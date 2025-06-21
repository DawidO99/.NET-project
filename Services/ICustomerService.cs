using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManagementSystem.Models;

namespace CarWorkshopManagementSystem.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id); // Dodana metoda GetByIdAsync
        Task AddAsync(Customer customer);
    }
}