using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRawTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RawItemId = table.Column<int>(type: "int", nullable: false),
                    InboundMeter = table.Column<double>(type: "float", nullable: false),
                    InboundKg = table.Column<double>(type: "float", nullable: false),
                    Outbound = table.Column<double>(type: "float", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    PackagingStyleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawTransactions_PackagingStyles_PackagingStyleId",
                        column: x => x.PackagingStyleId,
                        principalTable: "PackagingStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RawTransactions_RawItems_RawItemId",
                        column: x => x.RawItemId,
                        principalTable: "RawItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RawTransactions_StakeholderInfos_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "StakeholderInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawTransactions_PackagingStyleId",
                table: "RawTransactions",
                column: "PackagingStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTransactions_RawItemId",
                table: "RawTransactions",
                column: "RawItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTransactions_StakeholderId",
                table: "RawTransactions",
                column: "StakeholderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawTransactions");
        }
    }
}
