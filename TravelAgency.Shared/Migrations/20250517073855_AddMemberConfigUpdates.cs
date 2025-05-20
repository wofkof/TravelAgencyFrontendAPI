using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberConfigUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "PASSPORT",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Passport");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "T_Member",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "PASSPORT",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Passport");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Passport",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "PASSPORT");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "T_Member",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Passport",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "PASSPORT");
        }
    }
}
