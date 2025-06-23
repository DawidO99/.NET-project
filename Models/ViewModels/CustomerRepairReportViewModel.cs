// Models/ViewModels/CustomerRepairReportViewModel.cs
using CarWorkshopManagementSystem.Models;
using System.Collections.Generic;
using System.Linq; // Dla LINQ
using System; // Dla DateTime

namespace CarWorkshopManagementSystem.Models.ViewModels
{
    public class CustomerRepairReportViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public DateTime GenerationDate { get; set; } = DateTime.Now;

        public List<VehicleReportData> Vehicles { get; set; } = new List<VehicleReportData>();

        // Właściwości sumaryczne (obliczone w momencie generowania ViewModelu)
        public decimal TotalReportLaborCost => Vehicles.Sum(v => v.ServiceOrders.Sum(so => so.Tasks.Sum(st => st.LaborCost)));
        public decimal TotalReportPartsCost => Vehicles.Sum(v => v.ServiceOrders.Sum(so => so.Tasks.Sum(st => st.UsedParts.Sum(up => up.Quantity * up.PartUnitPrice))));
        public decimal TotalReportCost => TotalReportLaborCost + TotalReportPartsCost;
    }

    public class VehicleReportData
    {
        public int VehicleId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int Year { get; set; }

        public List<ServiceOrderReportData> ServiceOrders { get; set; } = new List<ServiceOrderReportData>();

        // Właściwości sumaryczne dla pojazdu (obliczone w ViewModelu)
        public decimal TotalVehicleLaborCost => ServiceOrders.Sum(so => so.Tasks.Sum(st => st.LaborCost));
        public decimal TotalVehiclePartsCost => ServiceOrders.Sum(so => so.Tasks.Sum(st => st.UsedParts.Sum(up => up.Quantity * up.PartUnitPrice)));
        public decimal TotalVehicleCost => TotalVehicleLaborCost + TotalVehiclePartsCost;
    }

    public class ServiceOrderReportData
    {
        public int ServiceOrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public ServiceOrderStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string AssignedMechanicName { get; set; } = "Nieprzypisany";

        public List<ServiceTaskReportData> Tasks { get; set; } = new List<ServiceTaskReportData>();

        // Właściwości sumaryczne dla zlecenia (obliczone w ViewModelu)
        public decimal TotalOrderLaborCost => Tasks.Sum(st => st.LaborCost);
        public decimal TotalOrderPartsCost => Tasks.Sum(st => st.UsedParts.Sum(up => up.Quantity * up.PartUnitPrice));
        public decimal TotalOrderCost => TotalOrderLaborCost + TotalOrderPartsCost;
    }

    public class ServiceTaskReportData
    {
        public int ServiceTaskId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal LaborCost { get; set; }

        public List<UsedPartReportData> UsedParts { get; set; } = new List<UsedPartReportData>();
    }

    public class UsedPartReportData
    {
        public int UsedPartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public decimal PartUnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPartCost => PartUnitPrice * Quantity;
    }
}