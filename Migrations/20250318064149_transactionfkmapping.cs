using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class transactionfkmapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_form_supplier_id",
                table: "Transactions",
                column: "form_supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_from_site_id",
                table: "Transactions",
                column: "from_site_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_from_userid",
                table: "Transactions",
                column: "from_userid");

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
                name: "FK_Transactions_Sites_from_site_id",
                table: "Transactions",
                column: "from_site_id",
                principalTable: "Sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Suppliers_form_supplier_id",
                table: "Transactions",
                column: "form_supplier_id",
                principalTable: "Suppliers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Units_unit_type_id",
                table: "Transactions",
                column: "unit_type_id",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_from_userid",
                table: "Transactions",
                column: "from_userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Materials_material_id",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Sites_from_site_id",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Suppliers_form_supplier_id",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Units_unit_type_id",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_from_userid",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_form_supplier_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_from_site_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_from_userid",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_material_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_unit_type_id",
                table: "Transactions");
        }
    }
}
