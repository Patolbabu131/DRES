using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class transactionitmes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Materials_material_id",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Units_unit_type_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_material_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_unit_type_id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "gst",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "material_id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "total_value",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "unit_type_id",
                table: "Transactions",
                newName: "site_id");

            migrationBuilder.RenameColumn(
                name: "tex_type",
                table: "Transactions",
                newName: "transaction_type");

            migrationBuilder.AddColumn<int>(
                name: "received_by_user_id",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "recived_datetime",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "request_id",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Transaction_Items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transaction_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    unit_type_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tex_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst = table.Column<int>(type: "int", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction_Items", x => x.id);
                    table.ForeignKey(
                        name: "FK_Transaction_Items_Materials_material_id",
                        column: x => x.material_id,
                        principalTable: "Materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Items_Transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "Transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_Items_Units_unit_type_id",
                        column: x => x.unit_type_id,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Items_material_id",
                table: "Transaction_Items",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Items_transaction_id",
                table: "Transaction_Items",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Items_unit_type_id",
                table: "Transaction_Items",
                column: "unit_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction_Items");

            migrationBuilder.DropColumn(
                name: "received_by_user_id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "recived_datetime",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "request_id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "transaction_type",
                table: "Transactions",
                newName: "tex_type");

            migrationBuilder.RenameColumn(
                name: "site_id",
                table: "Transactions",
                newName: "unit_type_id");

            migrationBuilder.AddColumn<int>(
                name: "gst",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "material_id",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_value",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_material_id",
                table: "Transactions",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_unit_type_id",
                table: "Transactions",
                column: "unit_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Materials_material_id",
                table: "Transactions",
                column: "material_id",
                principalTable: "Materials",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Units_unit_type_id",
                table: "Transactions",
                column: "unit_type_id",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
