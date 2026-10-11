using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class PostVersionsCreatedAtLinkPostUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PostVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PostVersions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostVersions_PostId",
                table: "PostVersions",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostVersions_UserId",
                table: "PostVersions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostVersions_Posts_PostId",
                table: "PostVersions",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostVersions_Users_UserId",
                table: "PostVersions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostVersions_Posts_PostId",
                table: "PostVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostVersions_Users_UserId",
                table: "PostVersions");

            migrationBuilder.DropIndex(
                name: "IX_PostVersions_PostId",
                table: "PostVersions");

            migrationBuilder.DropIndex(
                name: "IX_PostVersions_UserId",
                table: "PostVersions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PostVersions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PostVersions");
        }
    }
}
