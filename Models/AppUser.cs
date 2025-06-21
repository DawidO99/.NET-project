// Models/AppUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // DODANE: Potrzebne do [NotMapped]

namespace CarWorkshopManagementSystem.Models
{
    public class AppUser : IdentityUser
    {
        // Dodaj te właściwości, jeśli chcesz przechowywać imię i nazwisko oddzielnie
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Ta właściwość oblicza pełne imię i nazwisko
        // [NotMapped] oznacza, że ta właściwość nie będzie mapowana do kolumny w bazie danych
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim(); // .Trim() usunie spację, jeśli jedno z pól jest puste

        // Opcjonalne relacje
        public ICollection<ServiceOrder>? AssignedServiceOrders { get; set; } = new List<ServiceOrder>();
        public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
}