// Services/ReportService.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text; // Dodano dla kodowania

namespace CarWorkshopManagementSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
            // Konfiguracja kodowania dla PdfSharpCore, aby obsługiwał polskie znaki
            //Encoding.RegisterProvider(PdfSharpCore.Utils.CodePagesEncodingProvider.Instance); //TODO
        }

        public async Task<byte[]> GenerateCustomerRepairReportPdfAsync(CustomerRepairReportViewModel reportData)
        {
            try
            {
                var document = new PdfDocument();
                document.Info.Title = $"Raport napraw klienta - {reportData.CustomerFullName}";

                // Dodajemy stronę
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fontHeading = new XFont("Arial", 20, XFontStyle.Bold);
                var fontSubHeading = new XFont("Arial", 14, XFontStyle.Bold);
                var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);

                double x = 40;
                double y = 40;
                double lineHeight = 14;

                // Tytuł raportu
                gfx.DrawString($"Raport napraw klienta: {reportData.CustomerFullName}", fontHeading, XBrushes.Black, x, y);
                y += lineHeight * 2;
                gfx.DrawString($"Numer telefonu: {reportData.CustomerPhoneNumber}", fontSubHeading, XBrushes.Black, x, y);
                y += lineHeight * 1.5;
                gfx.DrawString($"Data wygenerowania: {reportData.GenerationDate.ToLocalTime():yyyy-MM-dd HH:mm}", fontNormal, XBrushes.Gray, x, y);
                y += lineHeight * 2;

                // Suma całkowita dla klienta
                gfx.DrawString("PODSUMOWANIE DLA KLIENTA:", fontSubHeading, XBrushes.Black, x, y);
                y += lineHeight;
                gfx.DrawString($"Całkowity koszt robocizny: {reportData.TotalReportLaborCost:C}", fontBold, XBrushes.Black, x, y);
                y += lineHeight;
                gfx.DrawString($"Całkowity koszt części: {reportData.TotalReportPartsCost:C}", fontBold, XBrushes.Black, x, y);
                y += lineHeight;
                gfx.DrawString($"Całkowity koszt napraw: {reportData.TotalReportCost:C}", fontBold, XBrushes.Black, x, y);
                y += lineHeight * 2;

                // Dane pojazdów
                foreach (var vehicle in reportData.Vehicles)
                {
                    if (y + lineHeight * 10 > page.Height - 40) // Sprawdź, czy zmieści się na stronie, jeśli nie, dodaj nową
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        x = 40;
                        y = 40;
                    }

                    gfx.DrawString($"Pojazd: {vehicle.Brand} {vehicle.Model} ({vehicle.RegistrationNumber}) - {vehicle.Year}r. VIN: {vehicle.VIN}", fontSubHeading, XBrushes.DarkBlue, x, y);
                    y += lineHeight * 1.5;
                    gfx.DrawString($"Łączny koszt dla pojazdu: {vehicle.TotalVehicleCost:C}", fontBold, XBrushes.DarkBlue, x, y);
                    y += lineHeight * 1.5;

                    // Dane zleceń
                    foreach (var order in vehicle.ServiceOrders)
                    {
                        if (y + lineHeight * 8 > page.Height - 40) // Sprawdź, czy zmieści się na stronie
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            x = 40;
                            y = 40;
                        }

                        gfx.DrawString($"Zlecenie #{order.ServiceOrderId}: {order.Description}", fontBold, XBrushes.DarkGreen, x, y);
                        y += lineHeight;
                        gfx.DrawString($"Status: {order.Status.GetDisplayName()}, Mechanik: {order.AssignedMechanicName}", fontNormal, XBrushes.Black, x, y);
                        y += lineHeight;
                        gfx.DrawString($"Data utworzenia: {order.CreationDate.ToLocalTime():yyyy-MM-dd HH:mm}", fontNormal, XBrushes.Black, x, y);
                        y += lineHeight;
                        if (order.CompletionDate.HasValue)
                        {
                            gfx.DrawString($"Data zakończenia: {order.CompletionDate.Value.ToLocalTime():yyyy-MM-dd HH:mm}", fontNormal, XBrushes.Black, x, y);
                            y += lineHeight;
                        }
                        gfx.DrawString($"Koszt zlecenia: {order.TotalOrderCost:C}", fontBold, XBrushes.Black, x, y);
                        y += lineHeight * 1.5;

                        // Czynności serwisowe
                        if (order.Tasks.Any())
                        {
                            gfx.DrawString("Czynności:", fontBold, XBrushes.Black, x + 20, y);
                            y += lineHeight;
                            foreach (var task in order.Tasks)
                            {
                                if (y + lineHeight * 5 > page.Height - 40) // Sprawdź, czy zmieści się na stronie
                                {
                                    page = document.AddPage();
                                    gfx = XGraphics.FromPdfPage(page);
                                    x = 40;
                                    y = 40;
                                }
                                gfx.DrawString($"- {task.Description} (Robocizna: {task.LaborCost:C})", fontNormal, XBrushes.Black, x + 20, y);
                                y += lineHeight;

                                // Użyte części
                                if (task.UsedParts.Any())
                                {
                                    gfx.DrawString("Użyte części:", fontBold, XBrushes.Black, x + 40, y);
                                    y += lineHeight;
                                    foreach (var usedPart in task.UsedParts)
                                    {
                                        if (y + lineHeight * 3 > page.Height - 40) // Sprawdź, czy zmieści się na stronie
                                        {
                                            page = document.AddPage();
                                            gfx = XGraphics.FromPdfPage(page);
                                            x = 40;
                                            y = 40;
                                        }
                                        gfx.DrawString($"  - {usedPart.PartName} (Ilość: {usedPart.Quantity}, Cena jedn.: {usedPart.PartUnitPrice:C}, Łącznie: {usedPart.TotalPartCost:C})", fontNormal, XBrushes.Black, x + 40, y);
                                        y += lineHeight;
                                    }
                                }
                            }
                            y += lineHeight; // Odstęp po czynnościach
                        }
                    }
                    y += lineHeight; // Odstęp po pojeździe
                }

                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    _logger.LogInformation("Wygenerowano raport PDF dla klienta {CustomerFullName}.", reportData.CustomerFullName);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania raportu PDF dla klienta {CustomerFullName}.", reportData.CustomerFullName);
                throw;
            }
        }

        public async Task<byte[]> GenerateMonthlyRepairReportPdfAsync(List<MonthlyRepairReportViewModel> reportData, int year, int month)
        {
            try
            {
                var document = new PdfDocument();
                document.Info.Title = $"Miesięczny raport napraw - {month:00}/{year}";

                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fontHeading = new XFont("Arial", 20, XFontStyle.Bold);
                var fontSubHeading = new XFont("Arial", 14, XFontStyle.Bold);
                var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyle.Bold);

                double x = 40;
                double y = 40;
                double lineHeight = 14;

                // Tytuł raportu miesięcznego
                gfx.DrawString($"Miesięczny raport napraw: {month:00}/{year}", fontHeading, XBrushes.Black, x, y);
                y += lineHeight * 2;
                gfx.DrawString($"Data wygenerowania: {DateTime.Now.ToLocalTime():yyyy-MM-dd HH:mm}", fontNormal, XBrushes.Gray, x, y);
                y += lineHeight * 2;

                if (!reportData.Any())
                {
                    gfx.DrawString("Brak zleceń serwisowych w wybranym okresie.", fontNormal, XBrushes.Black, x, y);
                    y += lineHeight;
                }
                else
                {
                    // Suma całkowita dla miesiąca
                    var totalMonthLaborCost = reportData.Sum(o => o.TotalLaborCost);
                    var totalMonthPartsCost = reportData.Sum(o => o.TotalPartsCost);
                    var totalMonthOrderCost = reportData.Sum(o => o.TotalOrderCost);
                    var totalMonthOrders = reportData.Count;

                    gfx.DrawString("PODSUMOWANIE ZA MIESIĄC:", fontSubHeading, XBrushes.Black, x, y);
                    y += lineHeight;
                    gfx.DrawString($"Liczba zleceń: {totalMonthOrders}", fontBold, XBrushes.Black, x, y);
                    y += lineHeight;
                    gfx.DrawString($"Całkowity koszt robocizny: {totalMonthLaborCost:C}", fontBold, XBrushes.Black, x, y);
                    y += lineHeight;
                    gfx.DrawString($"Całkowity koszt części: {totalMonthPartsCost:C}", fontBold, XBrushes.Black, x, y);
                    y += lineHeight;
                    gfx.DrawString($"Całkowity koszt napraw: {totalMonthOrderCost:C}", fontBold, XBrushes.Black, x, y);
                    y += lineHeight * 2;

                    // Nagłówki tabeli dla zleceń
                    double tableY = y;
                    double col1 = x;
                    double col2 = col1 + 100;
                    double col3 = col2 + 150;
                    double col4 = col3 + 80;
                    double col5 = col4 + 80;
                    double col6 = col5 + 80;
                    double col7 = col6 + 80;

                    gfx.DrawString("Nr Zlecenia", fontBold, XBrushes.Black, col1, tableY);
                    gfx.DrawString("Klient", fontBold, XBrushes.Black, col2, tableY);
                    gfx.DrawString("Pojazd", fontBold, XBrushes.Black, col3, tableY);
                    gfx.DrawString("Status", fontBold, XBrushes.Black, col4, tableY);
                    gfx.DrawString("Mechanik", fontBold, XBrushes.Black, col5, tableY);
                    gfx.DrawString("Koszt Rob.", fontBold, XBrushes.Black, col6, tableY);
                    gfx.DrawString("Koszt Części", fontBold, XBrushes.Black, col7, tableY);
                    tableY += lineHeight * 1.5;

                    // Dane zleceń w tabeli
                    foreach (var order in reportData)
                    {
                        if (tableY + lineHeight > page.Height - 40) // Sprawdź, czy zmieści się wiersz
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            x = 40;
                            y = 40;
                            tableY = y + lineHeight * 2; // Odstęp na nagłówek
                            // Ponowne rysowanie nagłówków tabeli na nowej stronie
                            gfx.DrawString("Nr Zlecenia", fontBold, XBrushes.Black, col1, y + lineHeight);
                            gfx.DrawString("Klient", fontBold, XBrushes.Black, col2, y + lineHeight);
                            gfx.DrawString("Pojazd", fontBold, XBrushes.Black, col3, y + lineHeight);
                            gfx.DrawString("Status", fontBold, XBrushes.Black, col4, y + lineHeight);
                            gfx.DrawString("Mechanik", fontBold, XBrushes.Black, col5, y + lineHeight);
                            gfx.DrawString("Koszt Rob.", fontBold, XBrushes.Black, col6, y + lineHeight);
                            gfx.DrawString("Koszt Części", fontBold, XBrushes.Black, col7, y + lineHeight);
                        }

                        gfx.DrawString(order.OrderId.ToString(), fontNormal, XBrushes.Black, col1, tableY);
                        gfx.DrawString(order.CustomerFullName, fontNormal, XBrushes.Black, col2, tableY);
                        gfx.DrawString(order.VehicleDetails, fontNormal, XBrushes.Black, col3, tableY);
                        gfx.DrawString(order.Status.GetDisplayName(), fontNormal, XBrushes.Black, col4, tableY);
                        gfx.DrawString(order.AssignedMechanicName, fontNormal, XBrushes.Black, col5, tableY);
                        gfx.DrawString(order.TotalLaborCost.ToString("F2"), fontNormal, XBrushes.Black, col6, tableY); // Formatowanie F2 dla wyrównania
                        gfx.DrawString(order.TotalPartsCost.ToString("F2"), fontNormal, XBrushes.Black, col7, tableY); // Formatowanie F2 dla wyrównania
                        tableY += lineHeight;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    _logger.LogInformation("Wygenerowano miesięczny raport PDF za {Month}/{Year}.", month, year);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania miesięcznego raportu PDF za {Month}/{Year}.", month, year);
                throw;
            }
        }
    }
}