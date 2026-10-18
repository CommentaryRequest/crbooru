using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class AddPostVersionsParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewParent",
                table: "PostVersions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParentChanged",
                table: "PostVersions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewParent",
                table: "PostVersions");

            migrationBuilder.DropColumn(
                name: "ParentChanged",
                table: "PostVersions");
        }
    }
}
