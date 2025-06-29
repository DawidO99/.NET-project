﻿// Services/CustomerService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
// Usunieto CarWorkshopManagementSystem.DTOs, bo serwis pracuje na encjach
// Nie potrzebujemy tutaj mappera CustomerMapper, bo mapowanie DTO -> encja dzieje się w kontrolerze
// A encja -> DTO jest wykonywane tylko w kontrolerze dla Get, więc serwis nadal zwraca encję.

namespace CarWorkshopManagementSystem.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context) // Usunieto CustomerMapper z konstruktora
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

        public async Task UpdateAsync(Customer customer)
        {
            _context.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}