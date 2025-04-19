using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class ho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "forwarded_to_ho",
                table: "Material_Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "forwarded_to_ho",
                table: "Material_Requests");
        }
    }
}
