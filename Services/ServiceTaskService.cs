// Services/ServiceTaskService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Dodaj to using

namespace CarWorkshopManagementSystem.Services
{
    public class ServiceTaskService : IServiceTaskService
    {
        private readonly ApplicationDbContext _context;

        public ServiceTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateTaskAsync(ServiceTask serviceTask) // Implementacja
        {
            _context.ServiceTasks.Add(serviceTask);
            await _context.SaveChangesAsync();
        }
        // Dodaj tutaj inne implementacje metod z IServiceTaskService
    }
}