using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class OrderExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "T_Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Awaiting",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Unpaid");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "T_Order",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "T_Order");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "T_Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unpaid",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Awaiting");
        }
    }
}
