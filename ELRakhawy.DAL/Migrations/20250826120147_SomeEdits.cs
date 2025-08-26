using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELRakhawy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SomeEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialTransactionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormStyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackagingStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingStyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ContactNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialTransactionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderTypes_FinancialTransactionTypes_FinancialTransactionTypeId",
                        column: x => x.FinancialTransactionTypeId,
                        principalTable: "FinancialTransactionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YarnItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginYarnId = table.Column<int>(type: "int", nullable: true),
                    ManufacturerId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YarnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YarnItems_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YarnItems_OriginYarn",
                        column: x => x.OriginYarnId,
                        principalTable: "YarnItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PackagingStyleForms",
                columns: table => new
                {
                    PackagingStyleId = table.Column<int>(type: "int", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingStyleForms", x => new { x.PackagingStyleId, x.FormId });
                    table.ForeignKey(
                        name: "FK_PackagingStyleForms_FormStyles_FormId",
                        column: x => x.FormId,
                        principalTable: "FormStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackagingStyleForms_PackagingStyles_PackagingStyleId",
                        column: x => x.PackagingStyleId,
                        principalTable: "PackagingStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderInfoType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholdersInfoId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    StakeholderTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderInfoType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderInfoType_StakeholderInfos_StakeholdersInfoId",
                        column: x => x.StakeholdersInfoId,
                        principalTable: "StakeholderInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StakeholderInfoType_StakeholderTypes_StakeholderTypeId",
                        column: x => x.StakeholderTypeId,
                        principalTable: "StakeholderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakeholderTypeForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderTypeId = table.Column<int>(type: "int", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderTypeForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderTypeForms_FormStyles_FormId",
                        column: x => x.FormId,
                        principalTable: "FormStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StakeholderTypeForms_StakeholderTypes_StakeholderTypeId",
                        column: x => x.StakeholderTypeId,
                        principalTable: "StakeholderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YarnTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YarnItemId = table.Column<int>(type: "int", nullable: false),
                    Inbound = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Outbound = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    StakeholderTypeId = table.Column<int>(type: "int", nullable: false),
                    StakeholderId = table.Column<int>(type: "int", nullable: false),
                    PackagingStyleId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: true),
                    QuantityBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CountBalance = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YarnTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YarnTransactions_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YarnTransactions_PackagingStyles_PackagingStyleId",
                        column: x => x.PackagingStyleId,
                        principalTable: "PackagingStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YarnTransactions_StakeholderInfos_StakeholderId",
                        column: x => x.StakeholderId,
                        principalTable: "StakeholderInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YarnTransactions_StakeholderTypes_StakeholderTypeId",
                        column: x => x.StakeholderTypeId,
                        principalTable: "StakeholderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YarnTransactions_YarnItems_YarnItemId",
                        column: x => x.YarnItemId,
                        principalTable: "YarnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FinancialTransactionTypes",
                columns: new[] { "Id", "Comment", "Type" },
                values: new object[,]
                {
                    { 1, "في حالة الشراء", "دائن" },
                    { 2, "في حالة البيع", "مدين" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagingStyleForms_FormId",
                table: "PackagingStyleForms",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderInfoType_StakeholdersInfoId",
                table: "StakeholderInfoType",
                column: "StakeholdersInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderInfoType_StakeholderTypeId",
                table: "StakeholderInfoType",
                column: "StakeholderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderTypeForms_FormId",
                table: "StakeholderTypeForms",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderTypeForms_StakeholderTypeId",
                table: "StakeholderTypeForms",
                column: "StakeholderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderTypes_FinancialTransactionTypeId",
                table: "StakeholderTypes",
                column: "FinancialTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnItems_ManufacturerId",
                table: "YarnItems",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnItems_OriginYarnId",
                table: "YarnItems",
                column: "OriginYarnId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnTransactions_ManufacturerId",
                table: "YarnTransactions",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnTransactions_PackagingStyleId",
                table: "YarnTransactions",
                column: "PackagingStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnTransactions_StakeholderId",
                table: "YarnTransactions",
                column: "StakeholderId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnTransactions_StakeholderTypeId",
                table: "YarnTransactions",
                column: "StakeholderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnTransactions_YarnItemId",
                table: "YarnTransactions",
                column: "YarnItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagingStyleForms");

            migrationBuilder.DropTable(
                name: "StakeholderInfoType");

            migrationBuilder.DropTable(
                name: "StakeholderTypeForms");

            migrationBuilder.DropTable(
                name: "YarnTransactions");

            migrationBuilder.DropTable(
                name: "FormStyles");

            migrationBuilder.DropTable(
                name: "PackagingStyles");

            migrationBuilder.DropTable(
                name: "StakeholderInfos");

            migrationBuilder.DropTable(
                name: "StakeholderTypes");

            migrationBuilder.DropTable(
                name: "YarnItems");

            migrationBuilder.DropTable(
                name: "FinancialTransactionTypes");

            migrationBuilder.DropTable(
                name: "Manufacturers");
        }
    }
}
