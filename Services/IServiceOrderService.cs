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
        // W przyszłości: UpdateOrderAsync itp.
    }
}