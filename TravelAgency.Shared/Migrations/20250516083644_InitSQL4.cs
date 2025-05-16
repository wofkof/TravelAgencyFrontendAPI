using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitSQL4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "T_OrderParticipant",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler",
                column: "IdNumber",
                unique: true,
                filter: "[IdNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant");

            migrationBuilder.DropIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "T_OrderParticipant",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "T_MemberFavoriteTraveler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderParticipant_IdNumber",
                table: "T_OrderParticipant",
                column: "IdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_MemberFavoriteTraveler_IdNumber",
                table: "T_MemberFavoriteTraveler",
                column: "IdNumber",
                unique: true);
        }
    }
}
