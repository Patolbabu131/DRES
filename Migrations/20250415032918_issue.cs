using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class issue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "issued_quantity",
                table: "Material_Request_Item");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "issued_quantity",
                table: "Material_Request_Item",
                type: "int",
                nullable: true);
        }
    }
}
