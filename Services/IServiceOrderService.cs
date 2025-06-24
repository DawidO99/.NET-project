// Services/IServiceOrderService.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Models.ViewModels; // Dodano
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IServiceOrderService
    {
        Task<IEnumerable<ServiceOrder>> GetAllOrdersAsync();
        Task<ServiceOrder?> GetOrderByIdAsync(int id);
        Task CreateOrderAsync(ServiceOrder order);
        Task UpdateOrderAsync(ServiceOrder order);
        Task UpdateOrderStatusAsync(int orderId, ServiceOrderStatus newStatus);
        Task DeleteOrderAsync(int id);

        // DODANO: Metody do pobierania danych dla raportów
        Task<CustomerRepairReportViewModel?> GetCustomerRepairReportDataAsync(int customerId);
        Task<List<MonthlyRepairReportViewModel>> GetMonthlyRepairReportDataAsync(int year, int month);
    }
}