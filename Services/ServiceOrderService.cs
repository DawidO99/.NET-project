// Services/ServiceOrderService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // Dodaj to using, jeśli DateTime.Now będzie używane

namespace CarWorkshopManagementSystem.Services
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly ApplicationDbContext _context;

        public ServiceOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceOrder>> GetAllOrdersAsync()
        {
            return await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic)
                .ToListAsync();
        }

        public async Task<ServiceOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic)
                .Include(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Include(so => so.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task CreateOrderAsync(ServiceOrder order)
        {
            order.Status = ServiceOrderStatus.New; // Przypisanie wartości z enuma
            order.CreationDate = DateTime.Now;
            _context.ServiceOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(ServiceOrder order)
        {
            var existingOrder = await _context.ServiceOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == order.Id);

            if (existingOrder == null)
            {
                throw new InvalidOperationException($"Zlecenie o ID {order.Id} nie zostało znalezione.");
            }

            // Ważne: W kontrolerze przekazujemy już cały obiekt 'order', w którym status został zmieniony.
            // Tutaj musimy skopiować wartości, które chcemy zaktualizować.
            // Jeśli obiekt 'order' przekazywany do tej metody zawiera wszystkie zmienione pola,
            // to wystarczy ustawić jego stan na Modified.

            // Zapewniamy, że CreationDate nie zostanie nadpisane
            order.CreationDate = existingOrder.CreationDate;

            // Logika ustawiania CompletionDate na podstawie zmiany statusu
            if (order.Status == ServiceOrderStatus.Completed && existingOrder.Status != ServiceOrderStatus.Completed)
            {
                order.CompletionDate = DateTime.Now;
            }
            // Jeśli status zmienia się z "Zakończone" na inny, czyścimy CompletionDate
            else if (order.Status != ServiceOrderStatus.Completed && existingOrder.Status == ServiceOrderStatus.Completed)
            {
                order.CompletionDate = null;
            }
            // W przeciwnym razie (status nie zmienił się na Completed lub z Completed na inny), zachowujemy istniejącą CompletionDate
            else
            {
                order.CompletionDate = existingOrder.CompletionDate;
            }

            _context.Update(order); // EF Core zaczyna śledzić 'order' i oznacza go jako zmodyfikowany
            await _context.SaveChangesAsync();
        }
    }
}