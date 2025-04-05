using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class consumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAMaterial_ConsumptionctivityLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    site_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    confirmed_by = table.Column<int>(type: "int", nullable: true),
                    createdon = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAMaterial_ConsumptionctivityLogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserAMaterial_ConsumptionctivityLogs_Sites_site_id",
                        column: x => x.site_id,
                        principalTable: "Sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Material_Consumption_Item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    consumption_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    unit_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material_Consumption_Item", x => x.id);
                    table.ForeignKey(
                        name: "FK_Material_Consumption_Item_Materials_material_id",
                        column: x => x.material_id,
                        principalTable: "Materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_Consumption_Item_Units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_Consumption_Item_UserAMaterial_ConsumptionctivityLogs_consumption_id",
                        column: x => x.consumption_id,
                        principalTable: "UserAMaterial_ConsumptionctivityLogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Material_Consumption_Item_consumption_id",
                table: "Material_Consumption_Item",
                column: "consumption_id");

            migrationBuilder.CreateIndex(
                name: "IX_Material_Consumption_Item_material_id",
                table: "Material_Consumption_Item",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_Material_Consumption_Item_unit_id",
                table: "Material_Consumption_Item",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAMaterial_ConsumptionctivityLogs_site_id",
                table: "UserAMaterial_ConsumptionctivityLogs",
                column: "site_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Material_Consumption_Item");

            migrationBuilder.DropTable(
                name: "UserAMaterial_ConsumptionctivityLogs");
        }
    }
}
