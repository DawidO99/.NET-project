// Models/Vehicle.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka jest wymagana.")]
        [StringLength(50, ErrorMessage = "Marka nie może być dłuższa niż 50 znaków.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model jest wymagany.")]
        [StringLength(50, ErrorMessage = "Model nie może być dłuższy niż 50 znaków.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer VIN jest wymagany.")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "Numer VIN musi mieć 17 znaków.")]
        public string VIN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer rejestracyjny jest wymagany.")]
        [StringLength(10, ErrorMessage = "Numer rejestracyjny nie może być dłuższy niż 10 znaków.")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Rok musi być w zakresie 1900-2100.")]
        public int Year { get; set; }

        public string? ImageUrl { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!; // Upewnij się, że jest null!

        public ICollection<ServiceOrder> Orders { get; set; } = new List<ServiceOrder>();
    }
}