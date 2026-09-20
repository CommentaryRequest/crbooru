using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class UploadsFixRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_Uploads_UploadId",
                table: "MediaAssets");

            migrationBuilder.DropIndex(
                name: "IX_MediaAssets_UploadId",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "MediaAssets");

            migrationBuilder.CreateTable(
                name: "UploadMediaAssets",
                columns: table => new
                {
                    MediaAssetsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadMediaAssets", x => new { x.MediaAssetsId, x.UploadId });
                    table.ForeignKey(
                        name: "FK_UploadMediaAssets_MediaAssets_MediaAssetsId",
                        column: x => x.MediaAssetsId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadMediaAssets_Uploads_UploadId",
                        column: x => x.UploadId,
                        principalTable: "Uploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadMediaAssets_UploadId",
                table: "UploadMediaAssets",
                column: "UploadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadMediaAssets");

            migrationBuilder.AddColumn<int>(
                name: "UploadId",
                table: "MediaAssets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploadId",
                table: "MediaAssets",
                column: "UploadId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_Uploads_UploadId",
                table: "MediaAssets",
                column: "UploadId",
                principalTable: "Uploads",
                principalColumn: "Id");
        }
    }
}
