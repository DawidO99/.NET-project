// Models/ViewModels/MonthlyRepairReportViewModel.cs
using CarWorkshopManagementSystem.Models; // Dla ServiceOrderStatus
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CarWorkshopManagementSystem.Models.ViewModels
{
    public class MonthlyRepairReportViewModel
    {
        public int OrderId { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string VehicleDetails { get; set; } = string.Empty; // Marka Model (Rejestracja)
        public string ProblemDescription { get; set; } = string.Empty;
        public ServiceOrderStatus Status { get; set; }
        public string AssignedMechanicName { get; set; } = "Nieprzypisany";
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalLaborCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPartsCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalOrderCost { get; set; }
    }
}