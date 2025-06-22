// Models/ServiceOrder.cs
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // DODANO: Dla [Timestamp]

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
        public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.New;

        [Required(ErrorMessage = "Opis problemu jest wymagany.")]
        [StringLength(1000, ErrorMessage = "Opis problemu nie może przekraczać 1000 znaków.")] // DODANO: Ograniczenie długości
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.UtcNow; // ZMIENIONO: Z DateTime.Now na DateTime.UtcNow

        public DateTime? CompletionDate { get; set; } // DODANO: Pole na datę zakończenia zlecenia (nullowalne)

        public string? AssignedMechanicId { get; set; }
        public AppUser? AssignedMechanic { get; set; }

        [Required(ErrorMessage = "Pojazd jest wymagany.")] // DODANO: Walidacja dla VehicleId
        public int VehicleId { get; set; }

        // [ValidateNever] // Przeniesiono do kontrolera lub ApplicationDbContext, jeśli konieczne jest pominięcie walidacji.
        // Tutaj w modelu można by dodać, jeśli walidator będzie próbował walidować obiekt Vehicle.
        // Zwykle, jeśli VehicleId jest wymagane, Vehicle też będzie implicitnie wymagane przez relację,
        // chyba że używasz DTO lub ręcznie usuwasz z ModelState w kontrolerze.
        public Vehicle Vehicle { get; set; } = null!;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [Timestamp] // DODANO: Optymistyczna kontrola współbieżności
        public byte[]? RowVersion { get; set; }
    }
}