using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditDeleteVehicleActionsAndCorrectServiceOrderDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ServiceOrders",
                newName: "CreationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "ServiceOrders",
                newName: "CreatedAt");
        }
    }
}
