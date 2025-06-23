// Models/ViewModels/EditUserRolesViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models.ViewModels
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;

        // TA LINIA JEST KLUCZOWA: UserFullName powinno być nullowalne
        public string? UserFullName { get; set; } // Zmieniono z string = null! na string?

        public List<string> AllRoles { get; set; } = new List<string>();
        [Display(Name = "Przypisane role")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        public List<string> UserCurrentRoles { get; set; } = new List<string>();
    }
}