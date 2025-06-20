// Models/UsedPart.cs
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class UsedPart
    {
        public int Id { get; set; }

        public int PartId { get; set; }
        public Part Part { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        public int ServiceTaskId { get; set; }
        public ServiceTask Task { get; set; } = null!;
    }
}