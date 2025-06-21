using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Mechanik")]
    public class ServiceTasksController : Controller
    {
        private readonly IServiceTaskService _taskService;

        public ServiceTasksController(IServiceTaskService taskService)
        {
            _taskService = taskService;
        }

        // POST: /ServiceTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceOrderId,Description,LaborCost")] ServiceTask serviceTask)
        {
            // --- POPRAWKA TUTAJ ---
            // Usuwamy błąd walidacji dla właściwości nawigacyjnej, która nazywa się teraz "ServiceOrder"
            ModelState.Remove("ServiceOrder");
            ModelState.Remove("UsedParts");

            if (ModelState.IsValid)
            {
                await _taskService.CreateTaskAsync(serviceTask);
                // Używamy poprawnej właściwości: serviceTask.ServiceOrderId
                return RedirectToAction("Details", "ServiceOrders", new { id = serviceTask.ServiceOrderId });
            }

            // Używamy poprawnej właściwości: serviceTask.ServiceOrderId
            return RedirectToAction("Details", "ServiceOrders", new { id = serviceTask.ServiceOrderId });
        }
    }
}