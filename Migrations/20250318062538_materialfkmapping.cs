using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class materialfkmapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_site_id",
                table: "Stocks",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_unit_type_id",
                table: "Stocks",
                column: "unit_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_user_id",
                table: "Stocks",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Sites_site_id",
                table: "Stocks",
                column: "site_id",
                principalTable: "Sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Units_unit_type_id",
                table: "Stocks",
                column: "unit_type_id",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Users_user_id",
                table: "Stocks",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Sites_site_id",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Units_unit_type_id",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Users_user_id",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_site_id",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_unit_type_id",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_user_id",
                table: "Stocks");
        }
    }
}
