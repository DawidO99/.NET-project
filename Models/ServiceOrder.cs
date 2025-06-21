// Models/ServiceOrder.cs
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    // Definicja enuma dla statusów zlecenia
    public enum ServiceOrderStatus
    {
        [Display(Name = "Nowe")]
        New = 0,
        [Display(Name = "W trakcie")]
        InProgress = 1,
        [Display(Name = "Zakończone")]
        Completed = 2,
        [Display(Name = "Anulowane")]
        Canceled = 3
    }

    public class ServiceOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Status jest wymagany.")]
        // Zmieniamy typ Status na enum ServiceOrderStatus
        public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.New;

        [Required(ErrorMessage = "Opis problemu jest wymagany.")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        // Dodajemy właściwość dla daty zakończenia
        public DateTime? CompletionDate { get; set; } // Może być nullable, bo zlecenie może nie być zakończone

        public string? AssignedMechanicId { get; set; }
        public AppUser? AssignedMechanic { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}