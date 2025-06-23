// Data/DataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CarWorkshopManagementSystem.Models;
using System;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>(); // Pobierz logger

            // Tworzenie ról
            string[] roleNames = { "Admin", "Mechanik", "Recepcjonista" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Rola '{RoleName}' została pomyślnie utworzona.", roleName);
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            logger.LogError("Błąd tworzenia roli '{RoleName}': {ErrorDescription}", roleName, error.Description);
                        }
                    }
                }
            }

            // Tworzenie użytkownika Admina
            var adminEmail = "admin@workshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Jan", // Dodano imię
                    LastName = "Administrator" // Dodano nazwisko
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!"); // ZMIEŃ NA SILNIEJSZE HASŁO W PRODUKCJI!
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Użytkownik admina '{AdminEmail}' został pomyślnie utworzony.", adminEmail);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("Błąd tworzenia użytkownika admina '{AdminEmail}': {ErrorDescription}", adminEmail, error.Description);
                    }
                }
            }

            // DODANO: Tworzenie użytkowników Mechaników i Recepcjonistów
            await CreateUserWithRole(userManager, logger, "mechanik1@workshop.com", "MechanikPassword1!", "Mechanik", "Adam", "Mechanikowski");
            await CreateUserWithRole(userManager, logger, "mechanik2@workshop.com", "MechanikPassword2!", "Mechanik", "Piotr", "Naprawski");
            await CreateUserWithRole(userManager, logger, "recepcjonista1@workshop.com", "RecepcjaPassword1!", "Recepcjonista", "Anna", "Klientowa");
            await CreateUserWithRole(userManager, logger, "recepcjonista2@workshop.com", "RecepcjaPassword2!", "Recepcjonista", "Katarzyna", "Obsługowa");
        }

        // Metoda pomocnicza do tworzenia użytkownika i przypisywania mu roli
        private static async Task CreateUserWithRole(
            UserManager<AppUser> userManager,
            ILogger logger,
            string email,
            string password,
            string roleName,
            string firstName, // Dodano imię
            string lastName) // Dodano nazwisko
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName, // Ustaw imię
                    LastName = lastName // Ustaw nazwisko
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    logger.LogInformation("Użytkownik '{Email}' z rolą '{RoleName}' został pomyślnie utworzony.", email, roleName);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("Błąd tworzenia użytkownika '{Email}' z rolą '{RoleName}': {ErrorDescription}", email, roleName, error.Description);
                    }
                }
            }
        }
    }
}