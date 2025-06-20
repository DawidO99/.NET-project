// Models/ServiceTask.cs
using System.Collections.Generic;

namespace CarWorkshopManagementSystem.Models
{
    public class ServiceTask
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal LaborCost { get; set; }

        public int OrderId { get; set; }
        public ServiceOrder Order { get; set; } = null!;

        public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
    }
}