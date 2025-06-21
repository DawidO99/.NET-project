using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManagementSystem.Models;

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
                .HasOne(so => so.AssignedMechanic) // ServiceOrder ma jednego AssignedMechanic
                .WithMany(u => u.AssignedServiceOrders) // AssignedMechanic ma wiele AssignedServiceOrders
                .HasForeignKey(so => so.AssignedMechanicId) // Klucz obcy to AssignedMechanicId
                .IsRequired(false); // Mechanik może być początkowo nieprzypisany

            // Konfiguracja relacji dla Comment do AppUser (autor)
            builder.Entity<Comment>()
                .HasOne(c => c.Author) // Komentarz ma jednego Autora
                .WithMany(u => u.Comments) // Autor ma wiele komentarzy
                .HasForeignKey(c => c.AuthorId) // Klucz obcy to AuthorId
                .IsRequired(); // Autor jest zawsze wymagany

            // Konfiguracja relacji dla UsedPart do ServiceTask
            builder.Entity<UsedPart>()
                .HasOne(up => up.Task) // UsedPart ma jedną ServiceTask
                .WithMany(st => st.UsedParts) // ServiceTask ma wiele UsedPart
                .HasForeignKey(up => up.ServiceTaskId) // Klucz obcy to ServiceTaskId
                .OnDelete(DeleteBehavior.Cascade); // Przy usunięciu ServiceTask usuń powiązane UsedPart
        }
    }
}