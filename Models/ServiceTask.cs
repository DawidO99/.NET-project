using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWorkshopManagementSystem.Models
{
    public class ServiceTask
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public decimal LaborCost { get; set; }

        // --- POPRAWKA TUTAJ ---
        // Używamy spójnej nazwy dla klucza obcego i właściwości
        public int ServiceOrderId { get; set; }

        [ForeignKey("ServiceOrderId")]
        public ServiceOrder ServiceOrder { get; set; } = null!;

        public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
    }
}