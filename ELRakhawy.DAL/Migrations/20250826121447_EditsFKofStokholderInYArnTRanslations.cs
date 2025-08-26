using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditsFKofStokholderInYArnTRanslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YarnTransactions_StakeholderTypes_StakeholderTypeId",
                table: "YarnTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "StakeholderTypeId",
                table: "YarnTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_YarnTransactions_StakeholderTypes_StakeholderTypeId",
                table: "YarnTransactions",
                column: "StakeholderTypeId",
                principalTable: "StakeholderTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YarnTransactions_StakeholderTypes_StakeholderTypeId",
                table: "YarnTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "StakeholderTypeId",
                table: "YarnTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YarnTransactions_StakeholderTypes_StakeholderTypeId",
                table: "YarnTransactions",
                column: "StakeholderTypeId",
                principalTable: "StakeholderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
