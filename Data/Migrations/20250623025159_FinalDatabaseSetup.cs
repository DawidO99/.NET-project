using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalDatabaseSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ServiceOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceOrders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // USUNIĘTO TĄ LINIĘ, PONIEWAŻ KOLUMNA 'CompletionDate' JUŻ ISTNIEJE W BAZIE DANYCH
            // migrationBuilder.AddColumn<DateTime>(
            //     name: "CompletionDate",
            //     table: "ServiceOrders",
            //     type: "datetime2",
            //     nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceOrders",
                type: "rowversion", // To jest prawidłowy typ dla SQL Server dla Timestamp
                rowVersion: true,
                nullable: true); // Powinno być nullable, bo to nie Primary Key i jest to timestamp
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder routerBuilder) // Zmieniono nazwę parametru, aby uniknąć konfliktu
        {
            // Pamiętaj, że w Down() również musisz obsłużyć usunięcie kolumny RowVersion
            routerBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceOrders");

            // W Down() przywracasz poprzedni stan, więc jeśli CompletionDate było dodane wcześniej,
            // to w tej Down() go nie usuwasz, bo go nie dodałeś w Up().
            // Jeśli jednak chcesz, żeby to Down() było kompleksowe, musiałbyś tam uwzględnić.
            // Dla naszych celów skupiamy się na Up().

            routerBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            routerBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}