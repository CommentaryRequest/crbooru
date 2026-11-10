using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class CreateArtistCommentariesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistCommentary_Posts_PostId",
                table: "ArtistCommentary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistCommentary",
                table: "ArtistCommentary");

            migrationBuilder.RenameTable(
                name: "ArtistCommentary",
                newName: "ArtistCommentaries");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistCommentary_PostId",
                table: "ArtistCommentaries",
                newName: "IX_ArtistCommentaries_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistCommentaries",
                table: "ArtistCommentaries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistCommentaries_Posts_PostId",
                table: "ArtistCommentaries",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistCommentaries_Posts_PostId",
                table: "ArtistCommentaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistCommentaries",
                table: "ArtistCommentaries");

            migrationBuilder.RenameTable(
                name: "ArtistCommentaries",
                newName: "ArtistCommentary");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistCommentaries_PostId",
                table: "ArtistCommentary",
                newName: "IX_ArtistCommentary_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistCommentary",
                table: "ArtistCommentary",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistCommentary_Posts_PostId",
                table: "ArtistCommentary",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
