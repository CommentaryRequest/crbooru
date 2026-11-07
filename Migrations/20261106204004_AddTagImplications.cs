using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRbooru.Migrations
{
    /// <inheritdoc />
    public partial class AddTagImplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagImplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AntecedentId = table.Column<int>(type: "integer", nullable: false),
                    ConsequentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagImplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagImplications_Tags_AntecedentId",
                        column: x => x.AntecedentId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagImplications_Tags_ConsequentId",
                        column: x => x.ConsequentId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagImplications_AntecedentId",
                table: "TagImplications",
                column: "AntecedentId");

            migrationBuilder.CreateIndex(
                name: "IX_TagImplications_ConsequentId",
                table: "TagImplications",
                column: "ConsequentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagImplications");
        }
    }
}
