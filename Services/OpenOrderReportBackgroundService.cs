// Services/OpenOrderReportBackgroundService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public class OpenOrderReportBackgroundService : BackgroundService
    {
        private readonly ILogger<OpenOrderReportBackgroundService> _logger;
        // Tutaj będziemy wstrzykiwać serwisy potrzebne do generowania raportu i wysyłki maila
        // np. IServiceOrderService, IEmailSenderService (który będziemy musieli stworzyć)

        public OpenOrderReportBackgroundService(ILogger<OpenOrderReportBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Open Order Report Background Service is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("Open Order Report Background Service is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Open Order Report Background Service is working. Time: {Time}", DateTimeOffset.Now);

                // TUTAJ BĘDZIE LOGIKA GENEROWANIA RAPORTU I WYSYŁKI MAILA
                // Na razie tylko logujemy wiadomość.

                // Dla testów, uruchamiamy co 1 minutę. W produkcji zmienisz to na np. raz dziennie.
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Open Order Report Background Service has stopped.");
        }
    }
}