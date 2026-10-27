using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class AddPoolVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoolVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdaterId = table.Column<int>(type: "INTEGER", nullable: false),
                    PoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    NewState = table.Column<string>(type: "TEXT", nullable: false),
                    NewName = table.Column<string>(type: "TEXT", nullable: true),
                    NewDescription = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionChanged = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoolVersions_Pools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "Pools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoolVersions_Users_UpdaterId",
                        column: x => x.UpdaterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoolVersions_PoolId",
                table: "PoolVersions",
                column: "PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolVersions_UpdaterId",
                table: "PoolVersions",
                column: "UpdaterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoolVersions");
        }
    }
}
