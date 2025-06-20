// Services/ICustomerService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManagementSystem.Models; // Upewnij się, że to jest!

namespace CarWorkshopManagementSystem.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        // Będziemy dodawać więcej metod w przyszłości
    }
}