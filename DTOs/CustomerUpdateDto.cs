// DTOs/CustomerUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManagementSystem.DTOs
{
    public class CustomerUpdateDto
    {
        public int Id { get; set; } // DODANO: ID jest potrzebne w formularzu edycji

        [Required(ErrorMessage = "Imię i nazwisko są wymagane.")]
        [StringLength(100, ErrorMessage = "Imię i nazwisko nie mogą być dłuższe niż 100 znaków.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}