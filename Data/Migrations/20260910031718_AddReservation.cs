using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedForBorrowerId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ReservedForBorrowerId",
                table: "Items",
                column: "ReservedForBorrowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Borrowers_ReservedForBorrowerId",
                table: "Items",
                column: "ReservedForBorrowerId",
                principalTable: "Borrowers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Borrowers_ReservedForBorrowerId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ReservedForBorrowerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ReservedForBorrowerId",
                table: "Items");
        }
    }
}
