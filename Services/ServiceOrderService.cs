// Services/ServiceOrderService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // Dodano dla DateTime

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
            // ZMODYFIKOWANO: Dodano .AsSplitQuery()
            return await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic)
                .Include(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Include(so => so.Comments)
                    .ThenInclude(c => c.Author)
                .AsSplitQuery() // <<----- KLUCZOWA ZMIANA DLA WYDAJNOŚCI
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task CreateOrderAsync(ServiceOrder order)
        {
            order.Status = ServiceOrderStatus.New;
            order.CreationDate = DateTime.UtcNow;
            _context.ServiceOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(ServiceOrder order)
        {
            _context.Entry(order).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, ServiceOrderStatus newStatus)
        {
            var order = await _context.ServiceOrders.FindAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Service Order with ID {orderId} not found.");
            }

            order.Status = newStatus;
            if (newStatus == ServiceOrderStatus.Completed)
            {
                order.CompletionDate = DateTime.UtcNow;
            }
            else if (order.CompletionDate.HasValue && newStatus != ServiceOrderStatus.Completed)
            {
                order.CompletionDate = null;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _context.ServiceOrders.FindAsync(id);
            if (order != null)
            {
                _context.ServiceOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Service Order with ID {id} not found.");
            }
        }
    }
}