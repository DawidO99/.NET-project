﻿// Services/ICustomerService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWorkshopManagementSystem.Models;

namespace CarWorkshopManagementSystem.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer); // <-- DODANA METODA
        Task DeleteAsync(int id);
    }
}