using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class fkkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_material_id",
                table: "Stocks",
                column: "material_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Materials_material_id",
                table: "Stocks",
                column: "material_id",
                principalTable: "Materials",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Materials_material_id",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_material_id",
                table: "Stocks");
        }
    }
}
