using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class AddUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadId",
                table: "MediaAssets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Uploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UploaderId = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<byte>(type: "INTEGER", nullable: false),
                    StatusMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uploads_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploadId",
                table: "MediaAssets",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_UploaderId",
                table: "Uploads",
                column: "UploaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_Uploads_UploadId",
                table: "MediaAssets",
                column: "UploadId",
                principalTable: "Uploads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_Uploads_UploadId",
                table: "MediaAssets");

            migrationBuilder.DropTable(
                name: "Uploads");

            migrationBuilder.DropIndex(
                name: "IX_MediaAssets_UploadId",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "MediaAssets");
        }
    }
}
