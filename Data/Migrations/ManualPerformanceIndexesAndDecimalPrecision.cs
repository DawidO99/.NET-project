// Data/Migrations/ManualPerformanceIndexesAndDecimalPrecision.cs
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManagementSystem.Data.Migrations
{
    public partial class ManualPerformanceIndexesAndDecimalPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DODANO: Kolumna RowVersion do ServiceOrders (potwierdzona jako brakująca w bazie)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServiceOrders",
                type: "rowversion", // Specyficzny typ SQL Server dla optymistycznej współbieżności
                rowVersion: true,
                nullable: true); // Powinno być nullable

            // DODANO: Indeksy na kluczach obcych (FK), które były konsekwentnie pomijane przez Add-Migration
            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_VehicleId",
                table: "ServiceOrders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_AssignedMechanicId",
                table: "ServiceOrders",
                column: "AssignedMechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_ServiceOrderId",
                table: "ServiceTasks",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UsedParts_ServiceTaskId",
                table: "UsedParts",
                column: "ServiceTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UsedParts_PartId",
                table: "UsedParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OrderId",
                table: "Comments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            // DODANO: Jawne określenie precyzji dla typów decimal (eliminacja ostrzeżeń)
            // Te AlterColumn będą generowane tylko, jeśli obecny typ w bazie to np. "decimal(18,0)" lub "float"
            // lub jeśli nie ma ustawionej precyzji. Jeśli już jest decimal(18,2), to nie będzie zmiany.
            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "Parts",
                type: "decimal(18, 2)",
                nullable: false, // lub nullable: true jeśli taki był model
                oldClrType: typeof(decimal), // Użyj starego typu
                oldType: "decimal(18,2)", // Upewnij się, że to jest stary typ z bazy lub schema
                oldNullable: false); // Upewnij się, że to jest stara wartość nullable z bazy

            migrationBuilder.AlterColumn<decimal>(
                name: "LaborCost",
                table: "ServiceTasks",
                type: "decimal(18, 2)",
                nullable: false, // lub nullable: true jeśli taki był model
                oldClrType: typeof(decimal), // Użyj starego typu
                oldType: "decimal(18,2)", // Upewnij się, że to jest stary typ z bazy lub schema
                oldNullable: false); // Upewnij się, że to jest stara wartość nullable z bazy

            // Pozostałe AlterColumn i CreateIndex dla VIN, RegistrationNumber, FullName, PhoneNumber, Name
            // zakładam, że te operacje już były w poprzednich, zaaplikowanych migracjach,
            // więc nie dodaję ich tutaj, aby uniknąć błędu "kolumna już istnieje".
            // Ta migracja skupia się TYLKO na BRAKUJĄCYCH elementach, które Add-Migration pomijał.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Musimy również cofnąć wszystkie zmiany.
            // To jest trudniejsze bez automatycznego generowania.
            // Obejmie to operacje DropColumn i DropIndex.

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_VehicleId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_AssignedMechanicId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTasks_ServiceOrderId",
                table: "ServiceTasks");

            migrationBuilder.DropIndex(
                name: "IX_UsedParts_ServiceTaskId",
                table: "UsedParts");

            migrationBuilder.DropIndex(
                name: "IX_UsedParts_PartId",
                table: "UsedParts");

            migrationBuilder.DropIndex(
                name: "IX_Comments_OrderId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments");

            // Odwracamy AlterColumn dla decimal. Należy podać poprzednie typy i nullable.
            // Sprawdź w bazie danych, jakie były poprzednie typy kolumn UnitPrice i LaborCost.
            migrationBuilder.AlterColumn<decimal>( // Zmień na poprzedni typ (np. float, lub decimal bez precyzji)
                name: "UnitPrice",
                table: "Parts",
                type: "decimal(18,0)", // PRZYKŁADOWY STARY TYP. UPEWNIJ SIĘ, JAKI BYŁ W TWOJEJ BAZIE
                nullable: false, // Lub true, zależnie od starej konfiguracji
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>( // Zmień na poprzedni typ
                name: "LaborCost",
                table: "ServiceTasks",
                type: "decimal(18,0)", // PRZYKŁADOWY STARY TYP. UPEWNIJ SIĘ, JAKI BYŁ W TWOJEJ BAZIE
                nullable: false, // Lub true, zależnie od starej konfiguracji
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}