// Interfaces/IServiceOrderService.cs
using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IServiceOrderService
    {
        Task<IEnumerable<ServiceOrder>> GetAllOrdersAsync();
        Task<ServiceOrder?> GetOrderByIdAsync(int id);
        Task CreateOrderAsync(ServiceOrder order);
        Task UpdateOrderAsync(ServiceOrder order); // <-- DODAJ TĘ LINIĘ
        // Task DeleteOrderAsync(int id); // Jeśli jeszcze nie masz, możesz dodać również to
    }
}