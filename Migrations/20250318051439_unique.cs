using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "unitsymbol",
                table: "Units",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "unitname",
                table: "Units",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "gst",
                table: "Suppliers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "material_name",
                table: "Materials",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Units_unitname",
                table: "Units",
                column: "unitname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_unitsymbol",
                table: "Units",
                column: "unitsymbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_gst",
                table: "Suppliers",
                column: "gst",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_material_name",
                table: "Materials",
                column: "material_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_unitname",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_unitsymbol",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_gst",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Materials_material_name",
                table: "Materials");

            migrationBuilder.AlterColumn<string>(
                name: "unitsymbol",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "unitname",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "gst",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "material_name",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
