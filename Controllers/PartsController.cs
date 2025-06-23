// Controllers/PartsController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // DODANO: Dla ILogger
using System; // DODANO: Dla Exception
using System.Linq; // DODANO: Dla SelectMany

namespace CarWorkshopManagementSystem.Controllers
{
    // Autoryzacja: tylko Admin i Recepcjonista mogą zarządzać częściami
    [Authorize(Roles = "Admin,Recepcjonista")]
    public class PartsController : Controller
    {
        private readonly IPartService _partService;
        private readonly ILogger<PartsController> _logger; // DODANO: Logger

        public PartsController(IPartService partService, ILogger<PartsController> logger) // ZMODYFIKOWANO
        {
            _partService = partService;
            _logger = logger; // Zainicjowanie loggera
        }

        // GET: Parts
        public async Task<IActionResult> Index()
        {
            try
            {
                var parts = await _partService.GetAllPartsAsync();
                _logger.LogInformation("Pobrano {Count} części z katalogu.", parts.Count());
                return View(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania wszystkich części.");
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania katalogu części.";
                return View(new List<Part>()); // Zwróć pustą listę w razie błędu
            }
        }

        // GET: Parts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Błąd: Brak ID części w żądaniu szczegółów.");
                return NotFound();
            }

            try
            {
                var part = await _partService.GetPartByIdAsync(id.Value);
                if (part == null)
                {
                    _logger.LogWarning("Część o ID {PartId} nie znaleziono podczas próby wyświetlenia szczegółów.", id.Value);
                    return NotFound();
                }
                _logger.LogInformation("Pobrano szczegóły części ID {PartId}.", id.Value);
                return View(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania szczegółów części ID {PartId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania szczegółów części.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Parts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Parts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,UnitPrice")] Part part)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _partService.CreatePartAsync(part);
                    _logger.LogInformation("Dodano nową część: {PartName} (ID: {PartId}).", part.Name, part.Id);
                    TempData["SuccessMessage"] = $"Część '{part.Name}' została pomyślnie dodana.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas dodawania części: {PartName}.", part.Name);
                    TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania części. Spróbuj ponownie.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas tworzenia części: {Errors}", string.Join("; ", errors));
                TempData["ErrorMessage"] = "Nie udało się dodać części. Sprawdź poprawność danych.";
            }
            return View(part);
        }

        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Błąd: Brak ID części w żądaniu edycji.");
                return NotFound();
            }

            try
            {
                var part = await _partService.GetPartByIdAsync(id.Value);
                if (part == null)
                {
                    _logger.LogWarning("Część o ID {PartId} nie znaleziono podczas próby edycji.", id.Value);
                    return NotFound();
                }
                _logger.LogInformation("Pobrano część ID {PartId} do edycji.", id.Value);
                return View(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania strony edycji części ID {PartId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania danych części do edycji.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Parts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,UnitPrice")] Part part)
        {
            if (id != part.Id)
            {
                _logger.LogWarning("Błąd: Niezgodność ID części w żądaniu edycji. URL ID: {UrlId}, Model ID: {ModelId}.", id, part.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _partService.UpdatePartAsync(part);
                    _logger.LogInformation("Zaktualizowano część: {PartName} (ID: {PartId}).", part.Name, part.Id);
                    TempData["SuccessMessage"] = $"Część '{part.Name}' została pomyślnie zaktualizowana.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Konflikt współbieżności podczas aktualizacji części {PartId}.", part.Id);
                    TempData["ErrorMessage"] = "Błąd: Dane części zostały zmienione przez innego użytkownika. Spróbuj ponownie.";
                    // Ponowne załadowanie danych i powrót do widoku edycji
                    var freshPart = await _partService.GetPartByIdAsync(id);
                    return View(freshPart);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji części: {PartName} (ID: {PartId}).", part.Name, part.Id);
                    TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas aktualizacji części. Spróbuj ponownie.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas edycji części: {Errors}", string.Join("; ", errors));
                TempData["ErrorMessage"] = "Nie udało się zaktualizować części. Sprawdź poprawność danych.";
            }
            return View(part);
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Błąd: Brak ID części w żądaniu usunięcia.");
                return NotFound();
            }

            try
            {
                var part = await _partService.GetPartByIdAsync(id.Value);
                if (part == null)
                {
                    _logger.LogWarning("Część o ID {PartId} nie znaleziono podczas próby usunięcia.", id.Value);
                    return NotFound();
                }
                _logger.LogInformation("Pobrano część ID {PartId} do usunięcia.", id.Value);
                return View(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania strony usuwania części ID {PartId}.", id.Value);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania danych części do usunięcia.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _partService.DeletePartAsync(id);
                _logger.LogInformation("Usunięto część ID {PartId}.", id);
                TempData["SuccessMessage"] = $"Część została pomyślnie usunięta.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Próba usunięcia nieistniejącej części o ID {PartId}.", id);
                TempData["ErrorMessage"] = $"Błąd: Część o ID {id} nie została znaleziona.";
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania części o ID {PartId}.", id);
                TempData["ErrorMessage"] = "Wystąpił nieoczekiwany błąd podczas usuwania części. Spróbuj ponownie.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}