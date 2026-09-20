using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class UploadsCapitalizeStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Uploads",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Uploads",
                newName: "status");
        }
    }
}
