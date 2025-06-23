// Models/ViewModels/UserWithRolesViewModel.cs
using CarWorkshopManagementSystem.Models; // Ważne: dla AppUser
using System.Collections.Generic;

namespace CarWorkshopManagementSystem.Models.ViewModels // Zmieniono przestrzeń nazw na Models.ViewModels
{
    public class UserWithRolesViewModel
    {
        public AppUser User { get; set; } = null!; // Reprezentuje użytkownika
        public List<string> Roles { get; set; } = new List<string>(); // Lista nazw ról tego użytkownika
    }
}