// Models/Part.cs
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class Part
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
    }
}