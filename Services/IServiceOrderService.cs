// Services/IServiceOrderService.cs
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
        Task UpdateOrderAsync(ServiceOrder order); // DODANO: Metoda do aktualizacji ogólnych danych zlecenia
        Task UpdateOrderStatusAsync(int orderId, ServiceOrderStatus newStatus); // DODANO: Metoda do zmiany statusu
        Task DeleteOrderAsync(int id); // DODANO: Metoda do usuwania zlecenia
    }
}