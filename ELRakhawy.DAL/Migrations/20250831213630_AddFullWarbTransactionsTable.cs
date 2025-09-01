using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFullWarbTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FullWarpBeamTransations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullWarpBeamId = table.Column<int>(type: "int", nullable: false),
                    Inbound = table.Column<double>(type: "float", nullable: false),
                    Outbound = table.Column<double>(type: "float", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    StakeholderTypeId = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullWarpBeamTransations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FullWarpBeamTransations_FullWarpBeams_FullWarpBeamId",
                        column: x => x.FullWarpBeamId,
                        principalTable: "FullWarpBeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FullWarpBeamTransations_StakeholderInfos_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "StakeholderInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FullWarpBeamTransations_StakeholderTypes_StakeholderTypeId",
                        column: x => x.StakeholderTypeId,
                        principalTable: "StakeholderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FullWarpBeamTransations_FullWarpBeamId",
                table: "FullWarpBeamTransations",
                column: "FullWarpBeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FullWarpBeamTransations_StakeholderId",
                table: "FullWarpBeamTransations",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_FullWarpBeamTransations_StakeholderTypeId",
                table: "FullWarpBeamTransations",
                column: "StakeholderTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FullWarpBeamTransations");
        }
    }
}
