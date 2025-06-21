using CarWorkshopManagementSystem.Models;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IServiceTaskService
    {
        Task CreateTaskAsync(ServiceTask task);
    }
}