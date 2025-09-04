using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricServicsesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Style = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricStyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricColors_FabricStyles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "FabricStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FabricDesigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Design = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricDesigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricDesigns_FabricStyles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "FabricStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FabricStudios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ColorId = table.Column<int>(type: "int", nullable: true),
                    DesignId = table.Column<int>(type: "int", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WatermarkedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricStudios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricStudios_FabricColors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "FabricColors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FabricStudios_FabricDesigns_DesignId",
                        column: x => x.DesignId,
                        principalTable: "FabricDesigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FabricStudios_FabricItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FabricItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricColors_StyleId",
                table: "FabricColors",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricDesigns_StyleId",
                table: "FabricDesigns",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricStudios_ColorId",
                table: "FabricStudios",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricStudios_DesignId",
                table: "FabricStudios",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricStudios_ItemId",
                table: "FabricStudios",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricStudios");

            migrationBuilder.DropTable(
                name: "FabricColors");

            migrationBuilder.DropTable(
                name: "FabricDesigns");

            migrationBuilder.DropTable(
                name: "FabricStyles");
        }
    }
}
