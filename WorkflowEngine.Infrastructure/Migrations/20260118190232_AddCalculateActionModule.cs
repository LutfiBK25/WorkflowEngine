using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculateActionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_calculate_action_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_type = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    locked_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_calculate_action_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_calculate_action_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_calculate_module_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    calculate_action_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    operator_id = table.Column<int>(type: "integer", nullable: false),
                    input1_is_constant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    input1_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    input1_value = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    input2_is_constant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    input2_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    input2_value = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    result_field_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_calculate_module_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_calculate_module_details_t_calculate_action_modules_calcu~",
                        column: x => x.calculate_action_id,
                        principalTable: "t_calculate_action_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_calculate_action_modules_application_id",
                table: "t_calculate_action_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_calculate_module_details_calculate_action_id",
                table: "t_calculate_module_details",
                column: "calculate_action_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_calculate_module_details");

            migrationBuilder.DropTable(
                name: "t_calculate_action_modules");
        }
    }
}
