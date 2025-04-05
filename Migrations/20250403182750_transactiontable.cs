using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class transactiontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Units_unit_id",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_unit_id",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "from_userid",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "initiated_by_userid",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "received_by_user_id",
                table: "Transactions",
                newName: "gst");

            migrationBuilder.AlterColumn<int>(
                name: "to_user_id",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "to_site_id",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "createdat",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "tex_type",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "updatedat",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdat",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "tex_type",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "total_value",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "updatedat",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "gst",
                table: "Transactions",
                newName: "received_by_user_id");

            migrationBuilder.AlterColumn<int>(
                name: "to_user_id",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "to_site_id",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "from_userid",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "initiated_by_userid",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "unit_id",
                table: "Materials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_unit_id",
                table: "Materials",
                column: "unit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Units_unit_id",
                table: "Materials",
                column: "unit_id",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
