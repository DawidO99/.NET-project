// Data/DataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Dodaj ten import
using CarWorkshopManagementSystem.Models; // Dodaj ten import dla AppUser
using System;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>(); // Używamy AppUser

            // Tworzenie ról
            string[] roleNames = { "Admin", "Mechanik", "Recepcjonista" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
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
                    EmailConfirmed = true // Ustaw na true, żeby nie musieć potwierdzać e-maila
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!"); // ZMIEŃ NA SILNIEJSZE HASŁO W PRODUKCJI!
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    // Logowanie błędów tworzenia użytkownika
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    foreach (var error in result.Errors)
                    {
                        logger.LogError($"Error creating admin user: {error.Description}");
                    }
                }
            }
        }
    }
}