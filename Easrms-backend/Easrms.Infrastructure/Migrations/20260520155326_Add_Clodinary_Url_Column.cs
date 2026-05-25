using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Easrms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Clodinary_Url_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "ServiceRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7046"),
                column: "RoleName",
                value: "Support");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "ServiceRequests");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("90a5eb56-0ddc-4187-9f32-8870f8fc7046"),
                column: "RoleName",
                value: "Support User");
        }
    }
}
