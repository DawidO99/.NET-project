// DTOs/CustomerDto.cs
namespace CarWorkshopManagementSystem.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty; // Bezpośrednie mapowanie
        public string PhoneNumber { get; set; } = string.Empty; // Bezpośrednie mapowanie
        public int NumberOfVehicles { get; set; } // Liczba pojazdów, obliczane w mapperze
    }
}