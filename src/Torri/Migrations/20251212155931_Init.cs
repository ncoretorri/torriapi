using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Torri.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Torrents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    HasMissingRegex = table.Column<bool>(type: "boolean", nullable: false),
                    IsRunning = table.Column<bool>(type: "boolean", nullable: false),
                    TorrentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TorrentName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torrents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerieMasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TorrentEntityId = table.Column<int>(type: "integer", nullable: false),
                    FixSeason = table.Column<int>(type: "integer", nullable: true),
                    Mask = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerieMasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerieMasks_Torrents_TorrentEntityId",
                        column: x => x.TorrentEntityId,
                        principalTable: "Torrents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileIndex = table.Column<int>(type: "integer", nullable: false),
                    TorrentEntityId = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    Wanted = table.Column<bool>(type: "boolean", nullable: false),
                    EpisodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoFiles_Torrents_TorrentEntityId",
                        column: x => x.TorrentEntityId,
                        principalTable: "Torrents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerieMasks_TorrentEntityId",
                table: "SerieMasks",
                column: "TorrentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Torrents_Hash",
                table: "Torrents",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoFiles_TorrentEntityId",
                table: "VideoFiles",
                column: "TorrentEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerieMasks");

            migrationBuilder.DropTable(
                name: "VideoFiles");

            migrationBuilder.DropTable(
                name: "Torrents");
        }
    }
}
