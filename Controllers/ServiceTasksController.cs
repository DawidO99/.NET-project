// Controllers/ServiceTasksController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // Dodaj to using
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Mechanik")]
    public class ServiceTasksController : Controller
    {
        private readonly IServiceTaskService _serviceTaskService; // Zmieniono nazwę pola dla spójności
        private readonly IPartService _partService; // DODANE: Serwis do zarządzania częściami
        private readonly IServiceOrderService _serviceOrderService; // DODANE: Serwis do zarządzania zleceniami (potrzebny np. do pobierania zlecenia przed przekierowaniem)

        // ZMODYFIKOWANY KONSTRUKTOR: Dodano IPartService i IServiceOrderService
        public ServiceTasksController(IServiceTaskService serviceTaskService, IPartService partService, IServiceOrderService serviceOrderService)
        {
            _serviceTaskService = serviceTaskService;
            _partService = partService; // Zainicjuj
            _serviceOrderService = serviceOrderService; // Zainicjuj
        }

        // POST: /ServiceTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ZMODYFIKOWANA SYGNATURA METODY: Dodano tablice dla wybranych PartId i ilości
        public async Task<IActionResult> Create([Bind("ServiceOrderId,Description,LaborCost")] ServiceTask serviceTask, int[] selectedPartIds, int[] quantities)
        {
            ModelState.Remove("ServiceOrder");
            // Usunięto ModelState.Remove("UsedParts"); - to było zbędne, ModelBinder powinien sobie poradzić z pustą kolekcją

            if (ModelState.IsValid)
            {
                // DODANA LOGIKA: Przetwarzanie wybranych części
                if (selectedPartIds != null && quantities != null && selectedPartIds.Length == quantities.Length)
                {
                    serviceTask.UsedParts = new List<UsedPart>(); // WAŻNE: Inicjalizacja kolekcji
                    for (int i = 0; i < selectedPartIds.Length; i++)
                    {
                        // Upewnij się, że wybrano część i podano ilość
                        if (selectedPartIds[i] > 0 && quantities[i] > 0)
                        {
                            serviceTask.UsedParts.Add(new UsedPart
                            {
                                PartId = selectedPartIds[i],
                                Quantity = quantities[i]
                                // Id, ServiceTaskId, Part i ServiceTask zostaną ustawione automatycznie przez EF Core
                            });
                        }
                    }
                }

                await _serviceTaskService.CreateTaskAsync(serviceTask);
                return RedirectToAction("Details", "ServiceOrders", new { id = serviceTask.ServiceOrderId });
            }

            // Jeśli walidacja się nie powiodła, możesz wyświetlić komunikat o błędzie
            TempData["ErrorMessage"] = "Nie udało się dodać czynności serwisowej. Sprawdź poprawność danych (np. opis i koszt robocizny).";
            // Jeśli chcesz, aby formularz wyświetlał się z błędami, musisz ponownie załadować dane do ViewBag.Parts
            // ale ponieważ przekierowujesz, TempData jest lepszym rozwiązaniem.
            return RedirectToAction("Details", "ServiceOrders", new { id = serviceTask.ServiceOrderId });
        }
    }
}