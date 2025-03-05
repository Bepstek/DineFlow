using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dineflow.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryDetails_Menus_MenuItemId",
                table: "InventoryDetails");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDetails_MenuItemId",
                table: "InventoryDetails");

            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "InventoryDetails");

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "InventoryDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "InventoryDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "InventoryDetails");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "InventoryDetails");

            migrationBuilder.AddColumn<int>(
                name: "MenuItemId",
                table: "InventoryDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_MenuItemId",
                table: "InventoryDetails",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryDetails_Menus_MenuItemId",
                table: "InventoryDetails",
                column: "MenuItemId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
