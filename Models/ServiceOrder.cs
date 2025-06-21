// Models/ServiceOrder.cs
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class ServiceOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Status jest wymagany.")]
        public string Status { get; set; } = "Nowe";

        [Required(ErrorMessage = "Opis problemu jest wymagany.")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.Now; // Nazwa jest 'CreationDate'

        public string? AssignedMechanicId { get; set; }
        public AppUser? AssignedMechanic { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}