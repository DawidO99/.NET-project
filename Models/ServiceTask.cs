using System; // Potrzebne dla niektórych typów (np. DateTime, jeśli dodasz w przyszłości)
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWorkshopManagementSystem.Models
{
    public class ServiceTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)] // Dobrze dodać, aby ograniczyć długość
        public string Description { get; set; } = string.Empty; // Dodane = string.Empty; dla bezpieczeństwa

        [Column(TypeName = "decimal(18, 2)")] // Dodane, aby zapewnić precyzję w bazie danych
        public decimal LaborCost { get; set; }

        // ***** TO JEST LINIA, KTÓREJ BRAKUJE W TWOIM KODZIE I POWODUJE BŁĄD CS0117 *****
        public ServiceTaskStatus Status { get; set; } // Ta właściwość jest kluczowa dla błędu 'ServiceTask' does not contain a definition for 'Status'

        // Relacja do ServiceOrder
        public int ServiceOrderId { get; set; }
        [ForeignKey("ServiceOrderId")]
        public ServiceOrder ServiceOrder { get; set; } = null!; // Zapewnia, że ServiceOrder nie będzie null, jeśli jest kluczem obcym

        // Kolekcja powiązanych UsedPart
        public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>(); // Zawsze inicjuj kolekcję!
    }
}