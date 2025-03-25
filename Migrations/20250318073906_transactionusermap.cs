using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class transactionusermap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_from_userid",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_from_userid",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_to_user_id",
                table: "Transactions",
                column: "to_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_to_user_id",
                table: "Transactions",
                column: "to_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_to_user_id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_to_user_id",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_from_userid",
                table: "Transactions",
                column: "from_userid");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_from_userid",
                table: "Transactions",
                column: "from_userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
