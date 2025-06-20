// Services/CustomerService.cs
using System.Collections.Generic;
using System.Linq; // Potrzebne dla .ToListAsync() i .FirstOrDefaultAsync()
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <<------ BARDZO WAŻNE! Ten import był powodem poprzednich błędów CS1061
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;

namespace CarWorkshopManagementSystem.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers.Include(c => c.Vehicles).ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }
    }
}