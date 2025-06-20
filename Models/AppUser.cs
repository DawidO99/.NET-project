// Models/AppUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CarWorkshopManagementSystem.Models
{
    public class AppUser : IdentityUser
    {
        // Opcjonalne relacje, które dodamy później, na razie puste kolekcje
        public ICollection<ServiceOrder>? AssignedServiceOrders { get; set; } = new List<ServiceOrder>();
        public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
}