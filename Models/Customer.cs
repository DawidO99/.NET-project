// Models/Customer.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię i nazwisko są wymagane.")]
        [StringLength(100, ErrorMessage = "Imię i nazwisko nie mogą być dłuższe niż 100 znaków.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}