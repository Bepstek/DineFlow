using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dineflow.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_MenuItemId",
                table: "TransactionDetails",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_Menus_MenuItemId",
                table: "TransactionDetails",
                column: "MenuItemId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_Menus_MenuItemId",
                table: "TransactionDetails");

            migrationBuilder.DropIndex(
                name: "IX_TransactionDetails_MenuItemId",
                table: "TransactionDetails");
        }
    }
}
