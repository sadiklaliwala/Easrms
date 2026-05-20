using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Easrms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SlaHistory_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EscalatedBy",
                table: "ServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedOn",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalationReason",
                table: "ServiceRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEscalated",
                table: "ServiceRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SLAHours",
                table: "RequestCategories",
                type: "int",
                nullable: false,
                defaultValue: 24);

            migrationBuilder.CreateTable(
                name: "RequestEscalationHistory",
                columns: table => new
                {
                    EscalationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EscalationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEscalationHistory", x => x.EscalationId);
                    table.ForeignKey(
                        name: "FK_RequestEscalationHistory_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestEscalationHistory_Users_EscalatedBy",
                        column: x => x.EscalatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_EscalatedBy",
                table: "ServiceRequests",
                column: "EscalatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RequestEscalationHistory_EscalatedBy",
                table: "RequestEscalationHistory",
                column: "EscalatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RequestEscalationHistory_RequestId",
                table: "RequestEscalationHistory",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_EscalatedBy",
                table: "ServiceRequests",
                column: "EscalatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_EscalatedBy",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "RequestEscalationHistory");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_EscalatedBy",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "EscalatedBy",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "EscalatedOn",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "EscalationReason",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "IsEscalated",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "SLAHours",
                table: "RequestCategories");
        }
    }
}
