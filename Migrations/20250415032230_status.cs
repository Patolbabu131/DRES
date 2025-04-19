using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "forwarded_to_ho",
                table: "Material_Requests");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Material_Request_Item");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Material_Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Material_Requests");

            migrationBuilder.AddColumn<bool>(
                name: "forwarded_to_ho",
                table: "Material_Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Material_Request_Item",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
