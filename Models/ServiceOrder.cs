// Models/ServiceOrder.cs
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // DODANO: Potrzebne dla metod LINQ (Sum)

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
        [StringLength(1000, ErrorMessage = "Opis problemu nie może przekraczać 1000 znaków.")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        public string? AssignedMechanicId { get; set; }
        public AppUser? AssignedMechanic { get; set; }

        [Required(ErrorMessage = "Pojazd jest wymagany.")]
        public int VehicleId { get; set; }

        public Vehicle Vehicle { get; set; } = null!;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // DODANO: Właściwości obliczeniowe dla kosztów zlecenia
        [NotMapped] // Ważne: ta właściwość nie będzie mapowana do kolumny w bazie danych
        [Display(Name = "Całkowity koszt robocizny")]
        public decimal TotalLaborCost => Tasks.Sum(t => t.LaborCost);

        [NotMapped] // Ważne: ta właściwość nie będzie mapowana do kolumny w bazie danych
        [Display(Name = "Całkowity koszt części")]
        public decimal TotalPartsCost => Tasks.Sum(t => t.UsedParts.Sum(up => up.Quantity * up.Part.UnitPrice));
        // UWAGA: Ta właściwość wymaga, aby UsedParts i Part były załadowane (przez Include/ThenInclude)

        [NotMapped] // Ważne: ta właściwość nie będzie mapowana do kolumny w bazie danych
        [Display(Name = "Całkowity koszt zlecenia")]
        public decimal TotalOrderCost => TotalLaborCost + TotalPartsCost;
    }
}