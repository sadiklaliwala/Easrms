using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Easrms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inital_PostgraceSQl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "request_categories",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_approval_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sla_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refresh_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    refresh_token_expiry_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    otp_code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    otp_expiry_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    profile_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_users_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_requests",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    priority = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 30, nullable: false),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_escalated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    escalated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    escalated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    escalation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attachment_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_requests", x => x.request_id);
                    table.ForeignKey(
                        name: "fk_service_requests_request_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "request_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_requests_users_assigned_to",
                        column: x => x.assigned_to,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_requests_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_requests_users_employee_id",
                        column: x => x.employee_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_requests_users_escalated_by",
                        column: x => x.escalated_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_auth_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auth_provider = table.Column<int>(type: "integer", nullable: false),
                    external_user_id = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_auth_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_auth_providers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "request_comments",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_by = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    comment_type = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_comments", x => x.comment_id);
                    table.ForeignKey(
                        name: "fk_request_comments_service_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "service_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_comments_users_comment_by",
                        column: x => x.comment_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "request_escalation_histories",
                columns: table => new
                {
                    escalation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    escalated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    escalated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    escalation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_escalation_histories", x => x.escalation_id);
                    table.ForeignKey(
                        name: "fk_request_escalation_histories_service_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "service_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_request_escalation_histories_users_escalated_by",
                        column: x => x.escalated_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "request_status_histories",
                columns: table => new
                {
                    history_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<int>(type: "integer", maxLength: 30, nullable: true),
                    new_status = table.Column<int>(type: "integer", maxLength: 30, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_status_histories", x => x.history_id);
                    table.ForeignKey(
                        name: "fk_request_status_histories_service_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "service_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_status_histories_users_changed_by",
                        column: x => x.changed_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "role_name" },
                values: new object[,]
                {
                    { new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7043"), "Admin" },
                    { new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7044"), "Manager" },
                    { new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7045"), "Employee" },
                    { new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7046"), "Support" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_request_categories_category_name",
                table: "request_categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_request_comments_comment_by",
                table: "request_comments",
                column: "comment_by");

            migrationBuilder.CreateIndex(
                name: "ix_request_comments_request_id",
                table: "request_comments",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "ix_request_escalation_histories_escalated_by",
                table: "request_escalation_histories",
                column: "escalated_by");

            migrationBuilder.CreateIndex(
                name: "ix_request_escalation_histories_request_id",
                table: "request_escalation_histories",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "ix_request_status_histories_changed_by",
                table: "request_status_histories",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_request_status_histories_request_id",
                table: "request_status_histories",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_assigned_to",
                table: "service_requests",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_category_id",
                table: "service_requests",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_closed_by",
                table: "service_requests",
                column: "closed_by");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_employee_id",
                table: "service_requests",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_escalated_by",
                table: "service_requests",
                column: "escalated_by");

            migrationBuilder.CreateIndex(
                name: "ix_service_requests_request_number",
                table: "service_requests",
                column: "request_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_providers_user_id",
                table: "user_auth_providers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_manager_id",
                table: "users",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "request_comments");

            migrationBuilder.DropTable(
                name: "request_escalation_histories");

            migrationBuilder.DropTable(
                name: "request_status_histories");

            migrationBuilder.DropTable(
                name: "user_auth_providers");

            migrationBuilder.DropTable(
                name: "service_requests");

            migrationBuilder.DropTable(
                name: "request_categories");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
