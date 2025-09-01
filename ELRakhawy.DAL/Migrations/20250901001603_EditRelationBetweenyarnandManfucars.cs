using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditRelationBetweenyarnandManfucars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YarnItems_Manufacturers_ManufacturerId",
                table: "YarnItems");

            migrationBuilder.DropIndex(
                name: "IX_YarnItems_ManufacturerId",
                table: "YarnItems");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "YarnItems");

            migrationBuilder.CreateTable(
                name: "YarnItemManufacturers",
                columns: table => new
                {
                    ManufacturersId = table.Column<int>(type: "int", nullable: false),
                    YarnItemsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YarnItemManufacturers", x => new { x.ManufacturersId, x.YarnItemsId });
                    table.ForeignKey(
                        name: "FK_YarnItemManufacturers_Manufacturers_ManufacturersId",
                        column: x => x.ManufacturersId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YarnItemManufacturers_YarnItems_YarnItemsId",
                        column: x => x.YarnItemsId,
                        principalTable: "YarnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YarnItemManufacturers_YarnItemsId",
                table: "YarnItemManufacturers",
                column: "YarnItemsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YarnItemManufacturers");

            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "YarnItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YarnItems_ManufacturerId",
                table: "YarnItems",
                column: "ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_YarnItems_Manufacturers_ManufacturerId",
                table: "YarnItems",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id");
        }
    }
}
