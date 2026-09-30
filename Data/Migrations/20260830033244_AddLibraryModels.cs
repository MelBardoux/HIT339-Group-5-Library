using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGenres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGenres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToyTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LibraryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateOnly>(type: "date", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    PublicationYear = table.Column<int>(type: "int", nullable: true),
                    PublicationYearUnknown = table.Column<bool>(type: "bit", nullable: true),
                    AuthorId = table.Column<int>(type: "int", nullable: true),
                    AlbumTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReleaseYear = table.Column<int>(type: "int", nullable: true),
                    ReleaseYearUnknown = table.Column<bool>(type: "bit", nullable: true),
                    MinimumAge = table.Column<int>(type: "int", nullable: true),
                    BatteryRequired = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMusic",
                columns: table => new
                {
                    ArtistsId = table.Column<int>(type: "int", nullable: false),
                    MusicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMusic", x => new { x.ArtistsId, x.MusicId });
                    table.ForeignKey(
                        name: "FK_ArtistMusic_Artists_ArtistsId",
                        column: x => x.ArtistsId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistMusic_Items_MusicId",
                        column: x => x.MusicId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookBookGenre",
                columns: table => new
                {
                    BooksId = table.Column<int>(type: "int", nullable: false),
                    GenresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookGenre", x => new { x.BooksId, x.GenresId });
                    table.ForeignKey(
                        name: "FK_BookBookGenre_BookGenres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "BookGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookGenre_Items_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicMusicFormat",
                columns: table => new
                {
                    FormatsId = table.Column<int>(type: "int", nullable: false),
                    MusicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicMusicFormat", x => new { x.FormatsId, x.MusicId });
                    table.ForeignKey(
                        name: "FK_MusicMusicFormat_Items_MusicId",
                        column: x => x.MusicId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicMusicFormat_MusicFormats_FormatsId",
                        column: x => x.FormatsId,
                        principalTable: "MusicFormats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicMusicGenre",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "int", nullable: false),
                    MusicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicMusicGenre", x => new { x.GenresId, x.MusicId });
                    table.ForeignKey(
                        name: "FK_MusicMusicGenre_Items_MusicId",
                        column: x => x.MusicId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicMusicGenre_MusicGenres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "MusicGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToyToyType",
                columns: table => new
                {
                    ToysId = table.Column<int>(type: "int", nullable: false),
                    TypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToyToyType", x => new { x.ToysId, x.TypesId });
                    table.ForeignKey(
                        name: "FK_ToyToyType_Items_ToysId",
                        column: x => x.ToysId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToyToyType_ToyTypes_TypesId",
                        column: x => x.TypesId,
                        principalTable: "ToyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMusic_MusicId",
                table: "ArtistMusic",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookGenre_GenresId",
                table: "BookBookGenre",
                column: "GenresId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_AuthorId",
                table: "Items",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicMusicFormat_MusicId",
                table: "MusicMusicFormat",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicMusicGenre_MusicId",
                table: "MusicMusicGenre",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_ToyToyType_TypesId",
                table: "ToyToyType",
                column: "TypesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistMusic");

            migrationBuilder.DropTable(
                name: "BookBookGenre");

            migrationBuilder.DropTable(
                name: "MusicMusicFormat");

            migrationBuilder.DropTable(
                name: "MusicMusicGenre");

            migrationBuilder.DropTable(
                name: "ToyToyType");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "BookGenres");

            migrationBuilder.DropTable(
                name: "MusicFormats");

            migrationBuilder.DropTable(
                name: "MusicGenres");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ToyTypes");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
