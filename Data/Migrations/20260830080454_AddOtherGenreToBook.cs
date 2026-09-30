using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherGenreToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OtherGenre",
                table: "Items",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherGenreText",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherGenre",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OtherGenreText",
                table: "Items");
        }
    }
}
