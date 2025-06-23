// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManagementSystem.Models;
using System.Reflection; // Wymagane dla GetCustomAttribute

namespace CarWorkshopManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<ServiceOrder> ServiceOrders { get; set; } = null!;
        public DbSet<ServiceTask> ServiceTasks { get; set; } = null!;
        public DbSet<Part> Parts { get; set; } = null!;
        public DbSet<UsedPart> UsedParts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Konfiguracja relacji dla ServiceOrder do AppUser (mechanik)
            builder.Entity<ServiceOrder>()
                .HasOne(so => so.AssignedMechanic)
                .WithMany(u => u.AssignedServiceOrders)
                .HasForeignKey(so => so.AssignedMechanicId)
                .IsRequired(false);

            // Konfiguracja relacji dla Comment do AppUser (autor)
            builder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .IsRequired();

            // Konfiguracja relacji dla UsedPart do ServiceTask
            builder.Entity<UsedPart>()
                .HasOne(up => up.Task)
                .WithMany(st => st.UsedParts)
                .HasForeignKey(up => up.ServiceTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // === DODANO: Indeksy dla optymalizacji zapytań i integralności danych ===
            // Indeksy na kluczach obcych (FK), aby przyspieszyć JOIN-y w zapytaniach typu GetOrderByIdAsync
            builder.Entity<ServiceOrder>()
                .HasIndex(so => so.VehicleId); // Używane w JOIN z Vehicles
            builder.Entity<ServiceOrder>()
                .HasIndex(so => so.AssignedMechanicId); // Używane w JOIN z AspNetUsers

            builder.Entity<ServiceTask>()
                .HasIndex(st => st.ServiceOrderId); // Używane w JOIN z ServiceOrders

            builder.Entity<UsedPart>()
                .HasIndex(up => up.ServiceTaskId); // Używane w JOIN z ServiceTasks
            builder.Entity<UsedPart>()
                .HasIndex(up => up.PartId); // Używane w JOIN z Parts

            builder.Entity<Comment>()
                .HasIndex(c => c.OrderId); // Używane w JOIN z ServiceOrders
            builder.Entity<Comment>()
                .HasIndex(c => c.AuthorId); // Używane w JOIN z AspNetUsers

            // Indeksy na często wyszukiwanych lub unikalnych kolumnach (zgodnie z wymaganiami projektu)
            builder.Entity<Vehicle>()
                .HasIndex(v => v.VIN) // VIN powinien być unikalny
                .IsUnique();
            builder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber); // Często wyszukiwany

            builder.Entity<Customer>()
                .HasIndex(c => c.FullName); // Często wyszukiwany
            builder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber); // Często wyszukiwany

            builder.Entity<Part>()
                .HasIndex(p => p.Name); // Często wyszukiwany

            // === DODANO: Jawne określenie precyzji dla typów decimal (eliminacja ostrzeżeń) ===
            builder.Entity<Part>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18, 2)"); // Standardowa precyzja dla walut

            builder.Entity<ServiceTask>()
                .Property(st => st.LaborCost)
                .HasColumnType("decimal(18, 2)"); // Standardowa precyzja dla walut
        }
        // Zauważ, że klasa EnumExtensions została przeniesiona do osobnego pliku Services/EnumExtensions.cs
        // Nie powinna znajdować się tutaj.
    }
}