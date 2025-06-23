// Controllers/ServiceOrdersController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Potrzebne do UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrzebne do SelectList i SelectListItem
using Microsoft.EntityFrameworkCore; // Potrzebne dla DbUpdateConcurrencyException
using Microsoft.Extensions.Logging; // Dodano dla ILogger
using System; // Dodano dla Exception i KeyNotFoundException
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Dodaj to, jeśli jeszcze nie masz
using System.Reflection; // DODANO: Wymagane dla GetCustomAttribute
using static CarWorkshopManagementSystem.Services.EnumExtensions;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista,Mechanik")]
    public class ServiceOrdersController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<AppUser> _userManager; // Serwis do zarządzania użytkownikami
        private readonly IPartService _partService; // Serwis do zarządzania częściami
        private readonly ILogger<ServiceOrdersController> _logger; // DODANO: Logger

        // ZMODYFIKOWANY KONSTRUKTOR: Dodano IPartService i ILogger
        public ServiceOrdersController(IServiceOrderService orderService, IVehicleService vehicleService, UserManager<AppUser> userManager, IPartService partService, ILogger<ServiceOrdersController> logger)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _userManager = userManager;
            _partService = partService; // Zainicjowanie serwisu części
            _logger = logger; // Zainicjowanie loggera
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
                return View(new List<ServiceOrder>()); // Zwróć pustą listę w razie błędu
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
            // ModelState.Remove("Vehicle"); // To było wcześniej potrzebne, jeśli model Vehicle miał walidację Required.
            // Jeśli Vehicle jest Required w modelu ServiceOrder i nie jest bindowane, ten ModelState.Remove może być konieczny.
            // Przy użyciu [ValidateNever] na właściwości nawigacyjnej, to może być zbędne, ale zostawiam jako bezpieczne.
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

                // Ładowanie dostępnych części dla dropdownów w formularzu dodawania czynności
                var parts = await _partService.GetAllPartsAsync();
                ViewBag.Parts = parts.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} ({p.UnitPrice:C})" // Formatowanie nazwy i ceny dla widoku
                }).ToList();

                // Dodanie dostępnych statusów do ViewBag dla dropdowna zmiany statusu
                // Używamy GetValues i GetNames aby uzyskać wartości enumów i ich nazw displayowych
                ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                       .Cast<ServiceOrderStatus>()
                                       .Select(s => new SelectListItem
                                       {
                                           Value = ((int)s).ToString(),
                                           Text = s.GetDisplayName() // Metoda rozszerzająca do pobierania DisplayName
                                       })
                                       .ToList();

                _logger.LogInformation("Pobrano szczegóły zlecenia ID {OrderId}.", id.Value);
                return View(serviceOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania szczegółów zlecenia ID {OrderId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania szczegółów zlecenia.";
                return RedirectToAction(nameof(Index)); // Przekieruj do listy zleceń
            }
        }

        // NOWA AKCJA: POST do zmiany statusu zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")] // Tylko Admin i Mechanik mogą zmieniać status
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

        // NOWA AKCJA: GET dla strony edycji zlecenia
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
            return View(serviceOrder);
        }

        // NOWA AKCJA: POST dla strony edycji zlecenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Recepcjonista")] // Tylko Admin i Recepcjonista mogą edytować ogólne dane zlecenia
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,Description,AssignedMechanicId,Status,RowVersion")] ServiceOrder serviceOrder)
        {
            if (id != serviceOrder.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Vehicle"); // Ponownie, jeśli Vehicle jest Required, może być potrzebne usunięcie z ModelState

            // Zachowaj daty, jeśli nie są bindowane z formularza
            // Pobieramy aktualny stan z bazy, aby zachować CreationDate i CompletionDate, które nie są bindowane z formularza
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                TempData["ErrorMessage"] = "Błąd: Zlecenie zostało usunięte przez innego użytkownika.";
                return NotFound();
            }

            serviceOrder.CreationDate = existingOrder.CreationDate;
            serviceOrder.CompletionDate = existingOrder.CompletionDate; // Zachowaj CompletionDate

            if (ModelState.IsValid)
            {
                try
                {
                    // Używamy UpdateOrderAsync, który wymaga, aby obiekt był śledzony lub abyśmy użyli _context.Entry(order).State = EntityState.Modified;
                    // W ServiceOrderService użyliśmy _context.Entry(order).State = EntityState.Modified;, więc to jest OK.
                    await _orderService.UpdateOrderAsync(serviceOrder);
                    TempData["SuccessMessage"] = "Zlecenie zostało pomyślnie zaktualizowane.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Jeśli wystąpi konflikt współbieżności, sprawdź, czy obiekt nadal istnieje
                    // (może został usunięty przez innego użytkownika)
                    if (await _orderService.GetOrderByIdAsync(serviceOrder.Id) == null)
                    {
                        TempData["ErrorMessage"] = "Błąd: Zlecenie zostało usunięte przez innego użytkownika.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Błąd: Zlecenie zostało zmienione przez innego użytkownika. Twoje zmiany nie zostały zapisane.";
                        _logger.LogWarning("Konflikt współbieżności podczas edycji zlecenia ID {OrderId}.", serviceOrder.Id);
                        // Jeśli chcesz wyświetlić formularz z bieżącymi danymi z bazy, możesz to zrobić tutaj,
                        // ale na razie rzucamy wyjątek dalej, aby globalny handler go złapał, jeśli nie ma innego mechanizmu.
                        throw; // Rzucamy wyjątek dalej, aby to była kwestia do dalszej diagnozy
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji zlecenia ID {OrderId}.", serviceOrder.Id);
                    TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas aktualizacji zlecenia.";
                }
            }
            await PopulateDropdowns(serviceOrder); // Ponownie wypełnij dropdowny w przypadku błędu
            return View(serviceOrder);
        }

        // NOWA AKCJA: GET dla strony usuwania zlecenia
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

        // NOWA AKCJA: POST dla usuwania zlecenia
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Recepcjonista")] // Tylko Admin i Recepcjonista mogą usuwać zlecenia
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

            // Tworzymy listę pojazdów w formacie "Marka Model (Rejestracja)"
            var vehicleList = vehicles.Select(v => new
            {
                v.Id,
                DisplayText = $"{v.Brand} {v.Model} ({v.RegistrationNumber})"
            });

            ViewBag.Vehicles = new SelectList(vehicleList, "Id", "DisplayText", model?.VehicleId);
            ViewBag.Mechanics = new SelectList(mechanics, "Id", "UserName", model?.AssignedMechanicId);
        }
    }

    // Klasa pomocnicza dla pobierania DisplayName z enumów
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
}