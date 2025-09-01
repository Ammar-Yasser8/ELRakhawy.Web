using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Edittables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FullWarpBeamTransations_FullWarpBeams_FullWarpBeamId",
                table: "FullWarpBeamTransations");

            migrationBuilder.DropForeignKey(
                name: "FK_FullWarpBeamTransations_StakeholderTypes_StakeholderTypeId",
                table: "FullWarpBeamTransations");

            migrationBuilder.DropIndex(
                name: "IX_FullWarpBeamTransations_FullWarpBeamId",
                table: "FullWarpBeamTransations");

            migrationBuilder.RenameColumn(
                name: "StakeholderTypeId",
                table: "FullWarpBeamTransations",
                newName: "FullWarpBeamItemId");

            migrationBuilder.RenameColumn(
                name: "FullWarpBeamId",
                table: "FullWarpBeamTransations",
                newName: "CountBalance");

            migrationBuilder.RenameIndex(
                name: "IX_FullWarpBeamTransations_StakeholderTypeId",
                table: "FullWarpBeamTransations",
                newName: "IX_FullWarpBeamTransations_FullWarpBeamItemId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Outbound",
                table: "FullWarpBeamTransations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "Inbound",
                table: "FullWarpBeamTransations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "FullWarpBeamTransations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityBalance",
                table: "FullWarpBeamTransations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_FullWarpBeamTransations_FullWarpBeams_FullWarpBeamItemId",
                table: "FullWarpBeamTransations",
                column: "FullWarpBeamItemId",
                principalTable: "FullWarpBeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FullWarpBeamTransations_FullWarpBeams_FullWarpBeamItemId",
                table: "FullWarpBeamTransations");

            migrationBuilder.DropColumn(
                name: "QuantityBalance",
                table: "FullWarpBeamTransations");

            migrationBuilder.RenameColumn(
                name: "FullWarpBeamItemId",
                table: "FullWarpBeamTransations",
                newName: "StakeholderTypeId");

            migrationBuilder.RenameColumn(
                name: "CountBalance",
                table: "FullWarpBeamTransations",
                newName: "FullWarpBeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FullWarpBeamTransations_FullWarpBeamItemId",
                table: "FullWarpBeamTransations",
                newName: "IX_FullWarpBeamTransations_StakeholderTypeId");

            migrationBuilder.AlterColumn<double>(
                name: "Outbound",
                table: "FullWarpBeamTransations",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "Inbound",
                table: "FullWarpBeamTransations",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "FullWarpBeamTransations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FullWarpBeamTransations_FullWarpBeamId",
                table: "FullWarpBeamTransations",
                column: "FullWarpBeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FullWarpBeamTransations_FullWarpBeams_FullWarpBeamId",
                table: "FullWarpBeamTransations",
                column: "FullWarpBeamId",
                principalTable: "FullWarpBeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FullWarpBeamTransations_StakeholderTypes_StakeholderTypeId",
                table: "FullWarpBeamTransations",
                column: "StakeholderTypeId",
                principalTable: "StakeholderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
