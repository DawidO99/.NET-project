// Models/ServiceOrder.cs
using System.Collections.Generic; // Potrzebne dla ICollection
using System; // Potrzebne dla DateTime

namespace CarWorkshopManagementSystem.Models
{
    // Na razie minimalna definicja
    public class ServiceOrder
    {
        public int Id { get; set; }
        public string Status { get; set; } = "New"; // Domyślny status
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AssignedMechanicId { get; set; }
        public AppUser? AssignedMechanic { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}