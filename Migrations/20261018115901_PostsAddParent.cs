using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class PostsAddParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Posts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ParentId",
                table: "Posts",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_ParentId",
                table: "Posts",
                column: "ParentId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_ParentId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ParentId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
