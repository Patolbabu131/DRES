using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DRES.Migrations
{
    /// <inheritdoc />
    public partial class request : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Material_Requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    site_id = table.Column<int>(type: "int", nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    requested_by = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approval_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    forwarded_to_ho = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material_Requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_Material_Requests_Sites_site_id",
                        column: x => x.site_id,
                        principalTable: "Sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Material_Request_Item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    unit_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    issued_quantity = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material_Request_Item", x => x.id);
                    table.ForeignKey(
                        name: "FK_Material_Request_Item_Material_Requests_request_id",
                        column: x => x.request_id,
                        principalTable: "Material_Requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Material_Request_Item_Materials_material_id",
                        column: x => x.material_id,
                        principalTable: "Materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Material_Request_Item_Units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Material_Request_Item_material_id",
                table: "Material_Request_Item",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_Material_Request_Item_request_id",
                table: "Material_Request_Item",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_Material_Request_Item_unit_id",
                table: "Material_Request_Item",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_Material_Requests_site_id",
                table: "Material_Requests",
                column: "site_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Material_Request_Item");

            migrationBuilder.DropTable(
                name: "Material_Requests");
        }
    }
}
