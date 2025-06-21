using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionDateAndUpdateOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // KROK 1: Dodajemy nową kolumnę CompletionDate
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "ServiceOrders",
                type: "datetime2",
                nullable: true); // Może być nullable

            // KROK 2: Przed zmianą typu kolumny Status, aktualizujemy istniejące dane.
            // Konwertujemy stringowe wartości na odpowiadające im wartości numeryczne enuma.
            // Zakładamy, że ServiceOrderStatus jest zdefiniowany jako:
            // New = 0, InProgress = 1, Completed = 2, Canceled = 3
            // UPEWNIJ SIĘ, ŻE TE MAPOWANIA SĄ ZGODNE Z TWOIM ENUMEM!
            migrationBuilder.Sql("UPDATE ServiceOrders SET Status = 0 WHERE Status = 'Nowe'");
            migrationBuilder.Sql("UPDATE ServiceOrders SET Status = 1 WHERE Status = 'W trakcie'");
            migrationBuilder.Sql("UPDATE ServiceOrders SET Status = 2 WHERE Status = 'Zakończone'");
            migrationBuilder.Sql("UPDATE ServiceOrders SET Status = 3 WHERE Status = 'Anulowane'");


            // KROK 3: Zmieniamy typ kolumny Status z nvarchar na int
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ServiceOrders",
                type: "int",
                nullable: false,
                defaultValue: 0, // Domyślna wartość dla ServiceOrderStatus.New (0)
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // W metodzie Down, zmieniamy typ Status z powrotem na string,
            // nie ma potrzeby konwertować danych, jeśli nie zamierzasz ich przywracać
            // lub jeśli chcesz po prostu usunąć kolumnę i dodać ją jako string.
            // Domyślne wartości z Add-Migration są często OK dla Down.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "ServiceOrders");
        }
    }
}