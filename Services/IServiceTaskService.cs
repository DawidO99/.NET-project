// Services/IServiceTaskService.cs
using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IServiceTaskService
    {
        Task CreateTaskAsync(ServiceTask serviceTask); // DODANE lub POPRAWIONE
        // Dodaj tutaj inne metody, jeśli istnieją, np. GetTaskByIdAsync, UpdateTaskAsync, DeleteTaskAsync
    }
}