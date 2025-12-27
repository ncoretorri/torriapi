using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torri.Migrations
{
    /// <inheritdoc />
    public partial class AddToKodiFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AddToKodi",
                table: "Torrents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddToKodi",
                table: "Torrents");
        }
    }
}
