// Mappers/CustomerMapper.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.DTOs;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;

namespace CarWorkshopManagementSystem.Mappers
{
    [Mapper]
    public partial class CustomerMapper
    {
        public partial Customer Map(CustomerCreateDto customerCreateDto);
        public partial void Map(CustomerUpdateDto customerUpdateDto, Customer customer);

        [MapProperty(nameof(Customer.Vehicles), nameof(CustomerDto.NumberOfVehicles), Use = nameof(MapNumberOfVehicles))]
        public partial CustomerDto Map(Customer customer);
        public partial List<CustomerDto> Map(List<Customer> customers);

        // DODANO: Mapowanie z encji Customer na CustomerUpdateDto
        public partial CustomerUpdateDto MapToUpdateDto(Customer customer); // DODANO

        private string MapFullName(string firstName, string lastName) => $"{firstName} {lastName}";
        private int MapNumberOfVehicles(ICollection<Vehicle> vehicles) => vehicles?.Count ?? 0;
    }
}