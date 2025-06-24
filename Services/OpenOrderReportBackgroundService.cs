// Services/OpenOrderReportBackgroundService.cs
using CarWorkshopManagementSystem.Models; // Dodano dla ServiceOrderStatus, AppUser
using Microsoft.Extensions.DependencyInjection; // Dodano dla IServiceScopeFactory
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq; // Dodano dla LINQ
using System.Threading;
using System.Threading.Tasks;
using static CarWorkshopManagementSystem.Services.EnumExtensions; // Dla GetDisplayName
using CarWorkshopManagementSystem.Models.ViewModels; // Dla MonthlyRepairReportViewModel
using Microsoft.AspNetCore.Identity; // Dla UserManager<AppUser>

namespace CarWorkshopManagementSystem.Services
{
    public class OpenOrderReportBackgroundService : BackgroundService
    {
        private readonly ILogger<OpenOrderReportBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Dodano: do tworzenia zakresów dla serwisów

        // ZMODYFIKOWANY KONSTRUKTOR
        public OpenOrderReportBackgroundService(ILogger<OpenOrderReportBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory; // Wstrzykujemy fabrykę zakresów
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Open Order Report Background Service is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("Open Order Report Background Service is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Open Order Report Background Service is working. Time: {Time}", DateTimeOffset.Now);

                try
                {
                    // Używamy IServiceScopeFactory do tworzenia nowego zakresu
                    // To jest kluczowe, ponieważ serwisy takie jak DbContext są zazwyczaj Scoped
                    // i nie można ich wstrzykiwać bezpośrednio do Singletonowego BackgroundService.
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var serviceOrderService = scope.ServiceProvider.GetRequiredService<IServiceOrderService>();
                        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                        var emailSenderService = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(); // Potrzebny do znalezienia admina

                        // 1. Pobierz dane o aktualnych (otwartych/w trakcie) zleceniach
                        var openOrders = await serviceOrderService.GetAllOrdersAsync();
                        openOrders = openOrders.Where(o => o.Status == ServiceOrderStatus.New || o.Status == ServiceOrderStatus.InProgress).ToList();

                        if (openOrders.Any())
                        {
                            // 2. Przekształć dane na format MonthlyRepairReportViewModel dla raportu PDF
                            // Mimo że to jest raport z OTWARTYCH zleceń, możemy użyć MonthlyRepairReportViewModel,
                            // aby skorzystać z istniejącego formatowania PDF.
                            var reportData = openOrders.Select(so => new MonthlyRepairReportViewModel
                            {
                                OrderId = so.Id,
                                CustomerFullName = so.Vehicle?.Customer?.FullName ?? "N/A",
                                VehicleDetails = $"{so.Vehicle?.Brand} {so.Vehicle?.Model} ({so.Vehicle?.RegistrationNumber})",
                                ProblemDescription = so.Description,
                                Status = so.Status,
                                AssignedMechanicName = so.AssignedMechanic?.FullName ?? "Nieprzypisany",
                                CreationDate = so.CreationDate,
                                CompletionDate = so.CompletionDate,
                                // Tutaj koszty będą 0, jeśli nie załadowaliśmy Tasks/UsedParts w GetAllOrdersAsync
                                // Aby mieć poprawne koszty, musielibyśmy pobrać je z ServiceOrderService.GetOrderByIdAsync
                                // dla każdego zlecenia osobno lub zmodyfikować GetAllOrdersAsync o zagnieżdżone include dla Tasks i UsedParts
                                // Dla uproszczenia na razie koszty mogą być 0 w tym raporcie "otwartych zleceń"
                                TotalLaborCost = 0m, // Tymczasowo 0, wymaga rozszerzenia GetAllOrdersAsync
                                TotalPartsCost = 0m, // Tymczasowo 0, wymaga rozszerzenia GetAllOrdersAsync
                                TotalOrderCost = 0m  // Tymczasowo 0, wymaga rozszerzenia GetAllOrdersAsync
                            }).ToList();

                            // UWAGA: Jeśli chcesz, aby TotalLaborCost i TotalPartsCost były poprawne,
                            // metoda GetAllOrdersAsync w ServiceOrderService musiałaby Includeować Tasks, UsedParts i Part.
                            // W przeciwnym razie, sumy będą zerowe, ponieważ dane nie będą załadowane.
                            // Na potrzeby tego raportu z otwartych zleceń, często wystarczy lista podstawowych danych.

                            // 3. Wygeneruj raport PDF
                            var pdfBytes = await reportService.GenerateMonthlyRepairReportPdfAsync(reportData, DateTime.Now.Year, DateTime.Now.Month); // Używamy miesięcznego raportu jako bazę
                            var fileName = $"Raport_Otwarte_Zlecenia_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                            // 4. Znajdź adres e-mail admina
                            // Możesz zdefiniować adres admina w appsettings.json lub znaleźć użytkownika z rolą "Admin"
                            var adminUser = await userManager.GetUsersInRoleAsync("Admin");
                            var adminEmail = adminUser.FirstOrDefault()?.Email;

                            if (!string.IsNullOrEmpty(adminEmail))
                            {
                                // 5. Wyślij e-mail z załącznikiem
                                var subject = "Automatyczny raport: Otwarte zlecenia serwisowe";
                                var message = $"W załączeniu znajduje się aktualny raport z otwartych zleceń serwisowych na dzień {DateTime.Now.ToLocalTime():yyyy-MM-dd HH:mm}.";
                                await emailSenderService.SendEmailAsync(adminEmail, subject, message, pdfBytes, fileName);
                                _logger.LogInformation("Raport otwartych zleceń został wysłany na e-mail admina: {AdminEmail}.", adminEmail);
                            }
                            else
                            {
                                _logger.LogWarning("Brak e-maila admina do wysłania raportu otwartych zleceń.");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Brak otwartych zleceń do raportowania.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas generowania lub wysyłania raportu otwartych zleceń.");
                }

                // Dla testów, uruchamiamy co 1 minutę.
                // W produkcji zmienisz to na bardziej realistyczny interwał, np. raz dziennie.
                // TimeSpan.FromDays(1)
                //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);// Dla testów, co minutę
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Open Order Report Background Service has stopped.");
        }
    }
}