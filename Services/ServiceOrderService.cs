// Services/ServiceOrderService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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
                .AsSplitQuery()
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

        public async Task<CustomerRepairReportViewModel?> GetCustomerRepairReportDataAsync(int customerId)
        {
            // Pozostawiamy to bez zmian, ponieważ struktura CustomerRepairReportViewModel
            // ma właściwości obliczeniowe (TotalReportPartsCost, TotalReportLaborCost),
            // które są obliczane już w C# po pobraniu danych,
            // więc tutaj nie ma zagnieżdżonych sum SQL.
            var customer = await _context.Customers
                .Where(c => c.Id == customerId)
                .Select(c => new CustomerRepairReportViewModel
                {
                    CustomerId = c.Id,
                    CustomerFullName = c.FullName,
                    CustomerPhoneNumber = c.PhoneNumber,
                    GenerationDate = DateTime.UtcNow,
                    Vehicles = c.Vehicles.Select(v => new VehicleReportData
                    {
                        VehicleId = v.Id,
                        Brand = v.Brand,
                        Model = v.Model,
                        VIN = v.VIN,
                        RegistrationNumber = v.RegistrationNumber,
                        Year = v.Year,
                        ServiceOrders = v.Orders.Select(so => new ServiceOrderReportData
                        {
                            ServiceOrderId = so.Id,
                            Description = so.Description,
                            Status = so.Status,
                            CreationDate = so.CreationDate,
                            CompletionDate = so.CompletionDate,
                            AssignedMechanicName = so.AssignedMechanic != null ? so.AssignedMechanic.FullName : "Nieprzypisany",
                            Tasks = so.Tasks.Select(st => new ServiceTaskReportData
                            {
                                ServiceTaskId = st.Id,
                                Description = st.Description,
                                LaborCost = st.LaborCost, // To jest ok, bo to jest koszt dla taska, nie suma sum
                                UsedParts = st.UsedParts.Select(up => new UsedPartReportData
                                {
                                    UsedPartId = up.Id,
                                    PartName = up.Part.Name,
                                    PartUnitPrice = up.Part.UnitPrice,
                                    Quantity = up.Quantity
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            return customer;
        }

        // ZMODYFIKOWANO: Implementacja metody do pobierania danych dla raportu miesięcznego
        public async Task<List<MonthlyRepairReportViewModel>> GetMonthlyRepairReportDataAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            // Koniec miesiąca (do ostatniego ticka)
            // Użyj DateTime.AddMonths(1).AddDays(-1) dla końca dnia miesiąca o 23:59:59.999
            // lub po prostu następnego miesiąca
            var endDate = startDate.AddMonths(1); // Początek następnego miesiąca, aby warunek < endDate działał poprawnie

            var orders = await _context.ServiceOrders
                .Where(so => so.CreationDate >= startDate && so.CreationDate < endDate) // Zmieniono na < endDate
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.AssignedMechanic)
                .Include(so => so.Tasks) // Dołącz Tasks
                    .ThenInclude(t => t.UsedParts) // Dołącz UsedParts
                        .ThenInclude(up => up.Part) // Dołącz Part
                .AsSplitQuery()
                .OrderBy(so => so.CreationDate)
                .ToListAsync(); // Pobieramy encje ServiceOrder z powiązanymi danymi

            // Teraz, po pobraniu danych do pamięci, obliczamy sumy
            var reportData = orders.Select(so => new MonthlyRepairReportViewModel
            {
                OrderId = so.Id,
                CustomerFullName = so.Vehicle.Customer.FullName,
                VehicleDetails = $"{so.Vehicle.Brand} {so.Vehicle.Model} ({so.Vehicle.RegistrationNumber})",
                ProblemDescription = so.Description,
                Status = so.Status,
                AssignedMechanicName = so.AssignedMechanic != null ? so.AssignedMechanic.FullName : "Nieprzypisany",
                CreationDate = so.CreationDate,
                CompletionDate = so.CompletionDate,
                // Obliczenia wykonane w pamięci (LINQ to Objects)
                TotalLaborCost = so.Tasks.Sum(t => t.LaborCost),
                TotalPartsCost = so.Tasks.Sum(t => t.UsedParts.Sum(up => up.Quantity * up.Part.UnitPrice)),
                TotalOrderCost = so.Tasks.Sum(t => t.LaborCost) + so.Tasks.Sum(t => t.UsedParts.Sum(up => up.Quantity * up.Part.UnitPrice))
            }).ToList();

            return reportData;
        }
    }
}