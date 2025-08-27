using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFullWarpBeamAndRawDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FullWarpBeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginYarnId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullWarpBeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FullWarpBeams_YarnItems_OriginYarnId",
                        column: x => x.OriginYarnId,
                        principalTable: "YarnItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RawItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WarpId = table.Column<int>(type: "int", nullable: false),
                    WeftId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawItems_FullWarpBeams_WarpId",
                        column: x => x.WarpId,
                        principalTable: "FullWarpBeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RawItems_YarnItems_WeftId",
                        column: x => x.WeftId,
                        principalTable: "YarnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FullWarpBeams_OriginYarnId",
                table: "FullWarpBeams",
                column: "OriginYarnId");

            migrationBuilder.CreateIndex(
                name: "IX_RawItems_WarpId",
                table: "RawItems",
                column: "WarpId");

            migrationBuilder.CreateIndex(
                name: "IX_RawItems_WeftId",
                table: "RawItems",
                column: "WeftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawItems");

            migrationBuilder.DropTable(
                name: "FullWarpBeams");
        }
    }
}
