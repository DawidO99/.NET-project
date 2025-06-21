using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehiclesToCustomerRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_OrderId",
                table: "ServiceTasks");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "ServiceTasks",
                newName: "ServiceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceTasks_OrderId",
                table: "ServiceTasks",
                newName: "IX_ServiceTasks_ServiceOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks",
                column: "ServiceOrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks");

            migrationBuilder.RenameColumn(
                name: "ServiceOrderId",
                table: "ServiceTasks",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceTasks_ServiceOrderId",
                table: "ServiceTasks",
                newName: "IX_ServiceTasks_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_OrderId",
                table: "ServiceTasks",
                column: "OrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
