using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public class ServiceTaskService : IServiceTaskService
    {
        private readonly ApplicationDbContext _context;

        public ServiceTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateTaskAsync(ServiceTask task)
        {
            _context.ServiceTasks.Add(task);
            await _context.SaveChangesAsync();
        }
    }
}