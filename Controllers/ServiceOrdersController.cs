// Controllers/ServiceOrdersController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection; // Wymagane dla GetCustomAttribute
using static CarWorkshopManagementSystem.Services.EnumExtensions; // Potrzebne dla GetDisplayName()

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista,Mechanik")]
    public class ServiceOrdersController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPartService _partService;
        private readonly ILogger<ServiceOrdersController> _logger;

        public ServiceOrdersController(IServiceOrderService orderService, IVehicleService vehicleService, UserManager<AppUser> userManager, IPartService partService, ILogger<ServiceOrdersController> logger)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _userManager = userManager;
            _partService = partService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                _logger.LogInformation("Pobrano {Count} zleceń serwisowych dla strony głównej.", orders.Count());
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich zleceń serwisowych.");
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania zleceń.";
                return View(new List<ServiceOrder>());
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,Description,AssignedMechanicId")] ServiceOrder serviceOrder)
        {
            ModelState.Remove("Vehicle");

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.CreateOrderAsync(serviceOrder);
                    _logger.LogInformation("Utworzono nowe zlecenie serwisowe (ID: {OrderId}) dla pojazdu {VehicleId}.", serviceOrder.Id, serviceOrder.VehicleId);
                    TempData["SuccessMessage"] = "Zlecenie zostało pomyślnie utworzone.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas tworzenia zlecenia serwisowego dla pojazdu {VehicleId}.", serviceOrder.VehicleId);
                    TempData["ErrorMessage"] = "Wystąpił błąd podczas tworzenia zlecenia.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas tworzenia zlecenia: {Errors}", string.Join("; ", errors));
                TempData["ErrorMessage"] = "Nie udało się utworzyć zlecenia. Sprawdź poprawność danych.";
            }

            await PopulateDropdowns(serviceOrder);
            return View(serviceOrder);
        }

        // GET: ServiceOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Błąd: Brak ID zlecenia w żądaniu szczegółów.");
                return NotFound();
            }

            try
            {
                var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);

                if (serviceOrder == null)
                {
                    _logger.LogWarning("Zlecenie serwisowe o ID {OrderId} nie znaleziono.", id.Value);
                    return NotFound();
                }

                var parts = await _partService.GetAllPartsAsync();
                ViewBag.Parts = parts.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} ({p.UnitPrice:C})"
                }).ToList();

                ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                       .Cast<ServiceOrderStatus>()
                                       .Select(s => new SelectListItem
                                       {
                                           Value = ((int)s).ToString(),
                                           Text = s.GetDisplayName()
                                       })
                                       .ToList();

                _logger.LogInformation("Pobrano szczegóły zlecenia ID {OrderId}.", id.Value);
                return View(serviceOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania szczegółów zlecenia ID {OrderId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania szczegółów zlecenia.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST do zmiany statusu zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> ChangeStatus(int orderId, ServiceOrderStatus newStatus)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                _logger.LogInformation("Status zlecenia ID {OrderId} zmieniono na {NewStatus}.", orderId, newStatus);
                TempData["SuccessMessage"] = $"Status zlecenia #{orderId} zmieniono na '{newStatus.GetDisplayName()}' pomyślnie.";
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Próba zmiany statusu dla nieistniejącego zlecenia ID {OrderId}.", orderId);
                TempData["ErrorMessage"] = $"Błąd: Zlecenie o ID {orderId} nie zostało znalezione.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zmiany statusu zlecenia ID {OrderId} na {NewStatus}.", orderId, newStatus);
                TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas zmiany statusu.";
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        // GET dla strony edycji zlecenia
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);
            if (serviceOrder == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(serviceOrder); // Wypełnij dropdowny dla formularza edycji
            // Populating ViewBag.Statuses explicitly for Edit (if PopulateDropdowns doesn't cover it)
            // Ta linia jest zbędna, bo PopulateDropdowns już to robi
            // ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
            // .Cast<ServiceOrderStatus>()
            // .Select(s => new SelectListItem { Value = ((int)s).ToString(), Text = s.GetDisplayName() })
            // .ToList();
            return View(serviceOrder);
        }

        // POST dla strony edycji zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Recepcjonista")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,Description,AssignedMechanicId,Status,RowVersion")] ServiceOrder serviceOrder)
        {
            if (id != serviceOrder.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Vehicle");

            // POBIERAMY istniejący obiekt, który jest ŚLEDZONY
            var existingOrder = await _orderService.GetOrderByIdAsync(id); // ZMODYFIKOWANO
            if (existingOrder == null)
            {
                TempData["ErrorMessage"] = "Błąd: Zlecenie zostało usunięte przez innego użytkownika.";
                return NotFound();
            }

            // Kopiujemy zmienione właściwości Z OBIEKTU Z FORMULARZA (serviceOrder)
            // DO OBIEKTU ŚLEDZONEGO (existingOrder)
            existingOrder.VehicleId = serviceOrder.VehicleId;
            existingOrder.Description = serviceOrder.Description;
            existingOrder.AssignedMechanicId = serviceOrder.AssignedMechanicId;
            existingOrder.Status = serviceOrder.Status;
            existingOrder.RowVersion = serviceOrder.RowVersion; // WAŻNE: Kopiuj RowVersion dla optymistycznej współbieżności

            // Nie kopiujemy CreationDate ani CompletionDate, bo powinny być zarządzane przez logikę serwisu
            // i nie powinny być zmieniane bezpośrednio z formularza.

            if (ModelState.IsValid)
            {
                try
                {
                    // TERAZ PRZEKAZUJEMY ŚLEDZONY OBIEKT DO SERWISU
                    await _orderService.UpdateOrderAsync(existingOrder); // ZMIENIONO: Przekazujemy existingOrder
                    TempData["SuccessMessage"] = "Zlecenie zostało pomyślnie zaktualizowane.";
                    return RedirectToAction("Details", new { id = existingOrder.Id }); // ZMODYFIKOWANO: Przekierowanie do Details

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _orderService.GetOrderByIdAsync(serviceOrder.Id) == null)
                    {
                        TempData["ErrorMessage"] = "Błąd: Zlecenie zostało usunięte przez innego użytkownika.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Błąd: Zlecenie zostało zmienione przez innego użytkownika. Twoje zmiany nie zostały zapisane.";
                        _logger.LogWarning("Konflikt współbieżności podczas edycji zlecenia ID {OrderId}.", serviceOrder.Id);
                        // Przy konflikcie, załaduj świeże dane i wróć do widoku, aby użytkownik widział aktualny stan
                        var freshOrder = await _orderService.GetOrderByIdAsync(id);
                        await PopulateDropdowns(freshOrder);
                        ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                               .Cast<ServiceOrderStatus>()
                                               .Select(s => new SelectListItem
                                               {
                                                   Value = ((int)s).ToString(),
                                                   Text = s.GetDisplayName()
                                               })
                                               .ToList();
                        return View(freshOrder); // Zwróć widok ze świeżymi danymi
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji zlecenia ID {OrderId}.", serviceOrder.Id);
                    TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas aktualizacji zlecenia.";
                }
            }
            // Jeśli ModelState.IsValid jest false lub inny nieobsłużony błąd (poza konfliktem)
            await PopulateDropdowns(serviceOrder);
            ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                   .Cast<ServiceOrderStatus>()
                                   .Select(s => new SelectListItem
                                   {
                                       Value = ((int)s).ToString(),
                                       Text = s.GetDisplayName()
                                   })
                                   .ToList();
            return View(serviceOrder);
        }

        // GET dla strony usuwania zlecenia
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            return View(serviceOrder);
        }

        // POST dla usuwania zlecenia
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Recepcjonista")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                _logger.LogInformation("Usunięto zlecenie ID {OrderId}.", id);
                TempData["SuccessMessage"] = $"Zlecenie #{id} zostało pomyślnie usunięte.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Próba usunięcia nieistniejącego zlecenia ID {OrderId}.", id);
                TempData["ErrorMessage"] = $"Błąd: Zlecenie o ID {id} nie zostało znalezione.";
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania zlecenia ID {OrderId}.", id);
                TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas usuwania zlecenia.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Prywatna metoda pomocnicza do ładowania danych dla dropdownów
        private async Task PopulateDropdowns(ServiceOrder? model = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");

            var vehicleList = vehicles.Select(v => new
            {
                v.Id,
                DisplayText = $"{v.Brand} {v.Model} ({v.RegistrationNumber})"
            });

            ViewBag.Vehicles = new SelectList(vehicleList, "Id", "DisplayText", model?.VehicleId);
            ViewBag.Mechanics = new SelectList(mechanics, "Id", "UserName", model?.AssignedMechanicId);

            ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                   .Cast<ServiceOrderStatus>()
                                   .Select(s => new SelectListItem
                                   {
                                       Value = ((int)s).ToString(),
                                       Text = s.GetDisplayName()
                                   })
                                   .ToList();
        }
    }

    // Klasa pomocnicza dla pobierania DisplayName z enumów
    // UWAGA: Ta klasa powinna być w osobnym pliku (np. Services/EnumExtensions.cs)
    // Usunięcie jej stąd jest KLUCZOWE, bo jest to błąd CS1022.
    // public static class EnumExtensions
    // {
    //     public static string GetDisplayName(this Enum enumValue)
    //     {
    //         return enumValue.GetType()
    //                         .GetMember(enumValue.ToString())
    //                         .FirstOrDefault()?
    //                         .GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?
    //                         .Name ?? enumValue.ToString();
    //     }
    // }
}