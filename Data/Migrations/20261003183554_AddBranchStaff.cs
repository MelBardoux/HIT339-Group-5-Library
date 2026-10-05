using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestedByUserId",
                table: "ItemTransfers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "StaffBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBranches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransfers_RequestedByUserId",
                table: "ItemTransfers",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBranches_BranchId",
                table: "StaffBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBranches_UserId",
                table: "StaffBranches",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTransfers_AspNetUsers_RequestedByUserId",
                table: "ItemTransfers",
                column: "RequestedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemTransfers_AspNetUsers_RequestedByUserId",
                table: "ItemTransfers");

            migrationBuilder.DropTable(
                name: "StaffBranches");

            migrationBuilder.DropIndex(
                name: "IX_ItemTransfers_RequestedByUserId",
                table: "ItemTransfers");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "ItemTransfers");
        }
    }
}
