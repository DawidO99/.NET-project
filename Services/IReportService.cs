// Services/IReportService.cs
using CarWorkshopManagementSystem.Models.ViewModels;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic; // Dodano using

namespace CarWorkshopManagementSystem.Services
{
    public interface IReportService
    {
        // Generowanie raportu dla klienta (US12)
        Task<byte[]> GenerateCustomerRepairReportPdfAsync(CustomerRepairReportViewModel reportData);

        // Generowanie raportu miesięcznego (US13)
        Task<byte[]> GenerateMonthlyRepairReportPdfAsync(List<MonthlyRepairReportViewModel> reportData, int year, int month);
    }
}