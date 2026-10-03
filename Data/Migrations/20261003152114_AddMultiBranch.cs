using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create library branches first so they can be referenced by other tables.
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            // Seed initial branches needed by existing inventory.
            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Name", "Address" },
                values: new object[,]
                {
                    { 1, "Darwin", "Darwin, NT" },
                    { 2, "Sydney", "Sydney, NSW" },
                    { 3, "Brisbane", "Brisbane, QLD" }
                });

            // Add BranchId to existing Items.
            // Existing items temporarily receive BranchId = 1.
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Distribute existing inventory across the three branches.
            // Uses the numeric portion of the library code to create
            // deterministic test data across the branches.
            migrationBuilder.Sql(@"
                UPDATE Items
                SET BranchId =
                    CASE TRY_CONVERT(int, RIGHT(LibraryCode, 4)) % 3
                        WHEN 0 THEN 1
                        WHEN 1 THEN 2
                        WHEN 2 THEN 3
                        ELSE 1
                    END;
            ");

            migrationBuilder.CreateTable(
                name: "ItemTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    FromBranchId = table.Column<int>(type: "int", nullable: false),
                    ToBranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTransfers", x => x.Id);

                    table.ForeignKey(
                        name: "FK_ItemTransfers_Branches_FromBranchId",
                        column: x => x.FromBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_ItemTransfers_Branches_ToBranchId",
                        column: x => x.ToBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_ItemTransfers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceptionDesks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionDesks", x => x.Id);

                    table.ForeignKey(
                        name: "FK_ReceptionDesks_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_BranchId",
                table: "Items",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransfers_FromBranchId",
                table: "ItemTransfers",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransfers_ItemId",
                table: "ItemTransfers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTransfers_ToBranchId",
                table: "ItemTransfers",
                column: "ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionDesks_BranchId",
                table: "ReceptionDesks",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Branches_BranchId",
                table: "Items",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Branches_BranchId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ItemTransfers");

            migrationBuilder.DropTable(
                name: "ReceptionDesks");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Items_BranchId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Items");
        }
    }
}
