// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CarWorkshopManagementSystem.Models;
using Microsoft.Extensions.Logging;
using CarWorkshopManagementSystem.Models.ViewModels;
using System; // Dodano dla Exception

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")] // Tylko użytkownicy z rolą "Admin" mają dostęp do tego kontrolera
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Admin/Index - Lista wszystkich użytkowników z ich rolami
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();

                var usersWithRoles = new List<UserWithRolesViewModel>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new UserWithRolesViewModel
                    {
                        User = user,
                        Roles = roles.ToList()
                    });
                }
                _logger.LogInformation("Administrator pobrał listę użytkowników z rolami.");
                return View(usersWithRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania listy użytkowników dla administratora.");
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania listy użytkowników.";
                return View(new List<UserWithRolesViewModel>());
            }
        }

        // GET: Admin/EditRoles/{userId} - Wyświetla formularz do edycji ról użytkownika
        public async Task<IActionResult> EditRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Błąd: Brak ID użytkownika w żądaniu edycji ról.");
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Użytkownik o ID {UserId} nie znaleziony podczas edycji ról.", userId);
                    return NotFound();
                }

                // Pobierz wszystkie dostępne role
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                // Pobierz role, które użytkownik już posiada
                var userRoles = await _userManager.GetRolesAsync(user);

                var model = new EditUserRolesViewModel // Będziemy musieli stworzyć ten ViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserFullName = user.FullName,
                    AllRoles = allRoles,
                    UserCurrentRoles = userRoles.ToList()
                };

                _logger.LogInformation("Administrator przygotował formularz edycji ról dla użytkownika {UserName} (ID: {UserId}).", user.UserName, userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania formularza edycji ról dla użytkownika ID {UserId}.", userId);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania formularza edycji ról.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/EditRoles/{userId} - Zapisuje zmienione role użytkownika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model) // Będziemy używać tego samego ViewModela dla POST
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Błąd walidacji podczas edycji ról dla użytkownika {UserName} (ID: {UserId}): {Errors}", model.UserName, model.UserId, string.Join("; ", errors));
                TempData["ErrorMessage"] = "Błąd walidacji. Sprawdź poprawność danych.";
                // Musisz ponownie załadować AllRoles, jeśli wracasz do widoku z błędem
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Użytkownik o ID {UserId} nie znaleziony podczas próby zapisu ról.", model.UserId);
                    TempData["ErrorMessage"] = "Błąd: Użytkownik nie został znaleziony.";
                    return NotFound();
                }

                // Pobierz aktualne role użytkownika
                var userRoles = await _userManager.GetRolesAsync(user);

                // Usuń role, których nie ma w nowych, wybranych rolach
                var rolesToRemove = userRoles.Except(model.SelectedRoles ?? new List<string>()).ToList();
                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    _logger.LogInformation("Usunięto role {RolesToRemove} dla użytkownika {UserName} (ID: {UserId}).", string.Join(", ", rolesToRemove), user.UserName, user.Id);
                }

                // Dodaj role, których nie ma w starych rolach użytkownika
                var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(userRoles).ToList();
                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    _logger.LogInformation("Dodano role {RolesToAdd} dla użytkownika {UserName} (ID: {UserId}).", string.Join(", ", rolesToAdd), user.UserName, user.Id);
                }

                TempData["SuccessMessage"] = $"Role użytkownika {user.UserName} zostały pomyślnie zaktualizowane.";
                _logger.LogInformation("Role użytkownika {UserName} (ID: {UserId}) zostały pomyślnie zaktualizowane.", user.UserName, user.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisu ról dla użytkownika {UserName} (ID: {UserId}).", model.UserName, model.UserId);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas aktualizacji ról użytkownika. Spróbuj ponownie.";
                // Musisz ponownie załadować AllRoles, jeśli wracasz do widoku z błędem
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }
        }
    }
}