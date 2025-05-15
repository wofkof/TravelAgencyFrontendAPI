using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgencyFrontendAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitSQL2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Email",
                table: "T_MemberFavoriteTraveler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_Phone",
                table: "T_MemberFavoriteTraveler",
                column: "Phone",
                unique: true);
        }
    }
}
