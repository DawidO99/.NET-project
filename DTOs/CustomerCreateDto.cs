// DTOs/CustomerCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.DTOs
{
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Imię i nazwisko są wymagane.")]
        [StringLength(100, ErrorMessage = "Imię i nazwisko nie mogą być dłuższe niż 100 znaków.")]
        public string FullName { get; set; } = string.Empty; // Zmieniono na FullName

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu.")]
        public string PhoneNumber { get; set; } = string.Empty; // Zmieniono na PhoneNumber
    }
}