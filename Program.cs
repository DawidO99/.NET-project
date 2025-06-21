using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Services;
using CarWorkshopManagementSystem.Models; // <<------ DODAJ TEN IMPORT DLA AppUser
using Microsoft.AspNetCore.Identity; // To już masz, OK
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Dodaj ten import dla IServiceScope

namespace CarWorkshopManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args) // <<------ ZMIEŃ NA async Task Main
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ZMIANA TUTAJ: Używamy naszej własnej klasy AppUser i dodajemy obsługę ról
            builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true) // <<------ ZMIEŃ IdentityUser NA AppUser
                .AddRoles<IdentityRole>() // <<------ DODAJ TĄ LINIĘ, ABY WŁĄCZYĆ OBSŁUGĘ RÓL
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            // Dodaj Swagger/OpenAPI (dla dokumentacji API, przydatne w development)
            builder.Services.AddEndpointsApiExplorer(); // <<------ DODAJ TĄ LINIĘ
            builder.Services.AddSwaggerGen(); // <<------ DODAJ TĄ LINIĘ

            // Rejestracja serwisów biznesowych
            builder.Services.AddScoped<ICustomerService, CustomerService>(); // To już masz, OK

            //Rejestracja serwisu pojazdów
            builder.Services.AddScoped<IVehicleService, VehicleService>();

            // Rejestracja serwisu zleceń serwisowych
            builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();

            // Rejestracja serwisu zadań serwisowych
            builder.Services.AddScoped<IServiceTaskService, ServiceTaskService>();

            // Rejestracja serwisu Komentarzy
            builder.Services.AddScoped<ICommentService, CommentService>();

            // Rejestracja serwisu części
            builder.Services.AddScoped<IPartService, PartService>();

            var app = builder.Build();

            // **********************************************
            // DODANIE SEEDINGU RÓL I UŻYTKOWNIKA ADMINA
            // Ten blok kodu powinien być ZARAZ PO app.Build(), a przed Configure the HTTP request pipeline.
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    await DataSeeder.SeedRolesAndAdminUser(serviceProvider);
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    }
}