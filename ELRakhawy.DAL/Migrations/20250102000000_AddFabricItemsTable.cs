using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginRawId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricItems_RawItems",
                        column: x => x.OriginRawId,
                        principalTable: "RawItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricItems_Item_Unique",
                table: "FabricItems",
                column: "Item",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricItems_OriginRawId",
                table: "FabricItems",
                column: "OriginRawId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricItems");
        }
    }
}