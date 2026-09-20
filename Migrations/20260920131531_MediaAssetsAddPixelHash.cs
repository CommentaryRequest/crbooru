using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class MediaAssetsAddPixelHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PixelHash",
                table: "MediaAssets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PixelHash",
                table: "MediaAssets");
        }
    }
}
