// Program.cs
using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Services;
using CarWorkshopManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;

using NLog.Web;
using NLog;
using NLog.Extensions.Logging;

namespace CarWorkshopManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Konfiguracja NLog przed zbudowaniem hosta
            var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            logger.Debug("Application starting up");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Konfiguracja logowania ASP.NET Core do używania NLog
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.Host.UseNLog();

                // Add services to the container.
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

                builder.Services.AddControllersWithViews();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // Rejestracja serwisów biznesowych
                builder.Services.AddScoped<ICustomerService, CustomerService>();
                builder.Services.AddScoped<IVehicleService, VehicleService>();
                builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
                builder.Services.AddScoped<IServiceTaskService, ServiceTaskService>();
                builder.Services.AddScoped<ICommentService, CommentService>();
                builder.Services.AddScoped<IPartService, PartService>();
                builder.Services.AddScoped<IReportService, ReportService>();
                builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();

                // Rejestracja BackgroundService
                builder.Services.AddHostedService<OpenOrderReportBackgroundService>(); // DODANO: Rejestracja usługi w tle

                var app = builder.Build();

                // **********************************************
                // DODANIE SEEDINGU RÓL I UŻYTKOWNIKA ADMINA
                using (var scope = app.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    try
                    {
                        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        await context.Database.MigrateAsync();
                        await DataSeeder.SeedRolesAndAdminUser(serviceProvider);
                    }
                    catch (Exception ex)
                    {
                        var serviceLogger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        serviceLogger.LogError(ex, "An error occurred while seeding the database or applying migrations.");
                    }
                }
                // **********************************************

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}