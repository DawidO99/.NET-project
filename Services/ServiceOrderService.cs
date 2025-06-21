using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // Ładujemy zlecenie, a wraz z nim powiązany pojazd, a z pojazdu powiązanego klienta.
            // To jest przykład "eager loading" - dociągania powiązanych danych.
            return await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic) // Dołączamy też dane mechanika
                .ToListAsync();
        }

        public async Task<ServiceOrder?> GetOrderByIdAsync(int id)
        {
            // To zapytanie pobiera jedno zlecenie i wszystkie jego powiązania
            return await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic)
                .Include(so => so.Tasks)       // Dołączamy listę czynności
                    .ThenInclude(t => t.UsedParts) // Do czynności dołączamy użyte części
                        .ThenInclude(up => up.Part)  // Do użytych części dołączamy dane z katalogu części
                .Include(so => so.Comments)  // Dołączamy listę komentarzy
                    .ThenInclude(c => c.Author)   // Do komentarzy dołączamy dane autora
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task CreateOrderAsync(ServiceOrder order)
        {
            order.Status = "Nowe"; // Zmienione z "New" dla spójności z modelem i językiem polskim
            order.CreationDate = DateTime.Now; // Zmienione z CreatedAt = DateTime.UtcNow
            _context.ServiceOrders.Add(order);
            await _context.SaveChangesAsync();
        }
    }


}