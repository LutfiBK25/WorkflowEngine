using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    version_build = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activate_on_start = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_compiled = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_activated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_applications", x => x.id);
                });

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
                name: "t_compare_action_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    operator_id = table.Column<int>(type: "integer", nullable: false),
                    input1_is_constant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    input1_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    input1_value = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    input2_is_constant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    input2_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    input2_value = table.Column<string>(type: "text", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_compare_action_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_compare_action_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_database_action_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sql_statement = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_database_action_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_database_action_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_dialog_action_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    dialog_type = table.Column<int>(type: "integer", nullable: false),
                    result_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    help1_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    help2_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    help3_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    options_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    list_module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mask_input = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    mask_character = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "*")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_dialog_action_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_dialog_action_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_field_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    field_type = table.Column<int>(type: "integer", nullable: false),
                    default_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_field_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_field_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_list_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    max_rows = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_list_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_list_modules_t_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "t_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_process_modules",
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
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_process_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_process_modules_t_applications_application_id",
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

            migrationBuilder.CreateTable(
                name: "t_process_module_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    label_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    action_type = table.Column<int>(type: "integer", nullable: true),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action_module_type = table.Column<int>(type: "integer", nullable: true),
                    pass_label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fail_label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    commented_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_process_module_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_t_process_module_details_t_process_modules_process_module_id",
                        column: x => x.process_module_id,
                        principalTable: "t_process_modules",
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

            migrationBuilder.CreateIndex(
                name: "IX_t_compare_action_modules_application_id",
                table: "t_compare_action_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_database_action_modules_application_id",
                table: "t_database_action_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_dialog_action_modules_application_id",
                table: "t_dialog_action_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_field_modules_application_id",
                table: "t_field_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_list_modules_application_id",
                table: "t_list_modules",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_process_module_details_process_module_id",
                table: "t_process_module_details",
                column: "process_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_process_modules_application_id",
                table: "t_process_modules",
                column: "application_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_calculate_module_details");

            migrationBuilder.DropTable(
                name: "t_compare_action_modules");

            migrationBuilder.DropTable(
                name: "t_database_action_modules");

            migrationBuilder.DropTable(
                name: "t_dialog_action_modules");

            migrationBuilder.DropTable(
                name: "t_field_modules");

            migrationBuilder.DropTable(
                name: "t_list_modules");

            migrationBuilder.DropTable(
                name: "t_process_module_details");

            migrationBuilder.DropTable(
                name: "t_calculate_action_modules");

            migrationBuilder.DropTable(
                name: "t_process_modules");

            migrationBuilder.DropTable(
                name: "t_applications");
        }
    }
}
