using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgencyFrontendAPI.Migrations
{
    /// <inheritdoc />
    public partial class EditMessageCreateAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "T_ChatRoom",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "T_ChatRoom",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()");
        }
    }
}
